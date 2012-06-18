"""KivorExport.py - transfer audio from Kivor to NAS (converting to flac).

Copyright (c) Rockfather 2009, all rights reserved.    
"""

import ftplib
import imp
import os
import re
import shutil
import socket
import subprocess
import sys
import time


class KivorExport:

    def __init__( self, aArgs ):
        "Initialise class data and execute transfer"
        self.seqs = '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz'
        self.seq       = 0
        self.xiva      = None
        self.ftp       = None
        self.discs     = []
        self.volumes   = []
        self.mediums   = []
        self.logfile   = open( 'KivorExport.txt', 'wt+' )
        self.failsfile = open( 'kivorExportFAILS.txt', 'wt+' )
        self.stats     = { 'discs':  0,
                           'tracks': 0,
                           'xfer':   0,
                           'skip':   0,
                           'fail':   0}

        self.Log( '' )
        self.Log( 'Kivor Export File Utility' )
        self.Log( '-------------------------' )
        self.Log( '' )
                
        try:
            kivorIp = aArgs[1]
            nasPath = os.path.normpath( aArgs[2] )
        except:
            self.ShowUsage()
            self.Shutdown()

        self.Log( 'Transferring files' )
        self.Log( '    from       %s' % kivorIp )
        self.Log( '    to         %s' % nasPath )
        self.Log( '    convert to FLAC' )
        self.Log( '' )
                        
        # check we can write to current dir (for temp file storage and logs)
        localPath = self.GetMainDir()
        self.Log( 'Checking WRITE access to %s' % localPath )
        if not self.CheckWritePerms( localPath ):
            self.Die( 'Unable to write to %s' % localPath )
    
        # check we can write to the output path on the NAS
        self.Log( 'Checking WRITE access to %s' % nasPath )
        if not self.CheckWritePerms( nasPath ):
            self.Die( 'Unable to write to NAS at %s' % nasPath )
    
        # create Xiva connection to Kivor
        port = 6789            
        self.Log( 'Connecting to Kivor Xiva server %s:%d' % (kivorIp, port ))
        self.xiva = self.XivaStart( kivorIp, port )
        if not self.xiva:
            self.Die( 'Unable to connect to Kivor on %s:%d' % (kivorIp, port ))
                
        # check we can ping Kivor over Xiva
        (ok, resp) = self.XivaSend( 'PING' )
        if not ok:
            self.Die( 'Kivor Xiva NOT responding on %s' % kivorIp )
            
        # Ensure Kivor not in standby
        (ok, resp) = self.XivaSend( 'STATUS', '<POWER><MODE>' )
        if '<POWER><MODE>RUN' not in resp:
            self.Log( 'Kivor in standby - restarting' )
            self.XivaSend( 'SYSTEM', '<POWER><MODE>RUN' )
            self.xiva.close()
            for i in range( 120, 0, -10 ):
                self.Log( '    Waiting for %d secs' % i )
                time.sleep( 10 )
                
            self.xiva = self.XivaStart( kivorIp, port )
            (ok, resp) = self.XivaSend( 'STATUS', '<POWER><MODE>' )
            if '<POWER><MODE>RUN' not in resp:
                self.Die( 'Kivor failed to come out of standby' )
            else:
                self.Log( 'Kivor now running' )
            
        # Read media data from Kivor DB - using a Xiva cache
        self.Log( '' )
        (ok, resp) = self.XivaSend( 'SEARCH','<CACHE><OPEN>MEDIA' )
        m = re.match( '(.*)<MARKER>(\d+) (\d+)<COUNT>(\d+)', resp )
        cacheId   = m.groups()[2]
        count = int( m.groups()[3] )
        next  = 1
        num   = 0
        
        self.Log( 'Reading info for %d discs' % count )
        while next <= count:
            req = '<CACHE><LIST><MARKER>0 %s<FROM>%d' % (cacheId, next)
            (ok, resp) = self.XivaSend( 'SEARCH', req )
            m = re.search( '(.*)<FROM>(\d+)<FOR>(\d+)<AT>(.*)', resp )
            next   += int( m.groups()[2] )
            data    = m.groups()[3]
            entries = data.split( '<AT>' )
            for entry in entries:
                num += 1           
                disc = self.GetDisc( entry, count, num )
                self.discs.append( disc )

        #**********************************************************************
        # To speed up development, save data from Xiva to file
        # import pickle                
        # f = open( 'discs.pic', 'w' )
        # p = pickle.Pickler( f )
        # p.dump( self.discs )
        # f.close()
        #**********************************************************************
        
        # cleanup Xiva and put Kivor into standby
        self.XivaSend( 'SEARCH', '<CACHE><CLOSE><MARKER>0 %s' % cacheId )
        self.XivaSend( 'SYSTEM', '<POWER><MODE>STANDBY' )
        self.xiva.close()
        self.xiva = None
        
        #**********************************************************************
        # To speed up development, save Xiva data from file
        # import pickle
        # f = open( 'discs.pic', 'r' )
        # u = pickle.Unpickler( f )
        # self.discs = u.load()
        # f.close()
        #**********************************************************************
                
        # now have all the data required from Xiva. Kivor is in standby to
        # permit FTP access required for the actual transfer operations
        
        # check we can FTP to the Kivor
        self.Log( '' )
        self.Log( 'Connecting to Kivor FTP server' )
        try:
            ftp = ftplib.FTP( kivorIp )
            ftp.login( 'root', 'aap' )
            if not 'tunboks' in ftp.getwelcome():
                self.Die( 'Bad response from Kivor FTP server' )
        except:
            self.Die( 'Failed to connect to Kivor FTP server' )

        # get list of data 'volumes from the Kivor            
        self.Log( '  Getting volumes from Kivor FTP' )
        ftp.cwd( 'audio' )
        ftp.dir( self.AudioDirCb )
        time.sleep( 1 )
        
        # within every volume there is 0 or more mediums containing audio data
        num = 0
        for volume in self.volumes:
            self.mediums = []
            ftp.dir( volume, self.VolumeDirCb )
            time.sleep( 1 )

            # every medium represents a 'disc' so get this data            
            for medium in self.mediums:
                discId = medium.split( 'medium' )[1]
                for disc in self.discs:
                    if disc['id'] == discId:
                        self.stats['discs'] += 1
                        break
                        
                self.Log( '' )
                self.Log( '    (%d of %d) %s: %s (%s) [DiscID %s]' % (self.stats['discs'],
                    count, disc['artist'], disc['album'], disc['genre'], disc['id']) )
                    
                # transfer all tracks across, transcoding en-route
                for track in disc['tracks']:
                    self.stats['tracks'] += 1
                    self.Log( '      %s: %s' % (track['number'], track['title']) )
                    
                    # construct file names for source, dest and work files
                    number = int( track['number'] )
                    title  = self.Filesafe( track['title'] )
                    artist = self.Filesafe( disc['artist'] )
                    album  = self.Filesafe( disc['album'] )
                    
                    src  = '%s/%s/%s' % ( volume, medium, 'track%02d.wav' % number )
                    wav  = '%02d - %s.wav' % (number, title) 
                    flac = '%02d - %s.flac' % (number, title) 
                    dest = os.path.normpath( '%s/%s/%s/%s' % \
                        (nasPath, artist[:50], album[:50], flac[:60]) )

                    # skip transfer for tracks where destination already exists                    
                    if os.path.exists( dest ):
                        self.stats['skip'] += 1
                        self.Log( '           -> skipping (destination exists)' )
                        continue

                    try:                    
                        # get file from Kivor using FTP
                        rxFile = open( wav, 'wb' )
                        ftp.retrbinary( 'RETR ' + src, rxFile.write )
                        rxFile.close()
                        self.Log( '           -> retrieved from Kivor' )
                        
                        # transcode the file to FLAC (locally)
                        cmd =  'flac -6 --totally-silent --replay-gain --delete-input-file'
                        cmd += ' -T "artist=%s"' % self.Tagsafe( disc['artist'] )
                        cmd += ' -T "title=%s"' % self.Tagsafe( track['title'] )
                        cmd += ' -T "album=%s"' % self.Tagsafe( disc['album'] )
                        cmd += ' -T "tracknumber=%s"' % self.Tagsafe( track['number'] )
                        cmd += ' -T "genre=%s"' % self.Tagsafe( disc['genre'] )
                        cmd += ' -T comment="Retrieved from Kivor"'
                        cmd += ' "%s"' % wav    
                        subprocess.call( cmd, shell=True )
                        self.Log( '           -> converted to FLAC' )
                     
                        # move transcoded file to NAS (create dirs as required)
                        if not os.path.exists( os.path.dirname( dest )):
                            os.makedirs( os.path.dirname( dest )) 
                        shutil.copyfile( flac, dest )
                        os.unlink( flac )                    
                        self.stats['xfer'] += 1
                        self.Log( '           -> moved to %s' % dest )
                    except Exception, err:
                        self.stats['fail'] += 1
                        self.Fail( disc, track, src, err )
                        try:
                            os.unlink( dest )
                        except:
                            pass
                        try:
                            os.unlink( flac )
                        except:
                            pass
                        try:
                            os.unlink( wav )
                        except:
                            pass
        self.ShowStats()                            
        self.Shutdown()      

    def Shutdown( self ):                
        "Cleanly shut down the transfer"                                
        if self.xiva:
            self.xiva.close()
        if self.ftp:
            self.ftp.quit()
        if self.logfile:
            self.logfile.close()
        if self.failsfile:
            self.failsfile.close()
        sys.exit( 0 )
        
    def AudioDirCb( self, aData ):
        if 'volume' in aData:
            aData.split()[-1]
            self.volumes.append( aData.split()[-1] )
            
    def VolumeDirCb( self, aData ):
        if 'medium' in aData:
            aData.split()[-1]
            self.mediums.append( aData.split()[-1] )        
        
    def XivaStart( self, aIp, aPort ):
        "Connect to Xiva server on Kivor, returns connected socket"
        try:
            sock = socket.socket( socket.AF_INET, socket.SOCK_STREAM, 0 )
            sock.connect( (aIp, aPort) )
        except:
            sock = None
        return sock

    def XivaSend( self, aCmd, aParms='' ):
        "Send specified Xiva message to Kivor, returns status and response"    
        self.seq  = (self.seq+1) % len( self.seqs)
        frm = 'bawbag'
        to  = 'server'
        chk = ''      # no need for optional checksum when using TCP comms   
        msg = '#%s#@%s@%s$%s$%s~%s\r\n' % (frm, to, self.seq, aCmd, aParms, chk) 
        self.xiva.send( msg )
        resp = self.xiva.recv( 1024 )
        
        m = re.match( '#%s#@%s@([\d|\w])\$ACK\$([\d|\w])<OK>(.*)~' % (to, frm), resp )
        if m:
            ok    = True
            reply = m.groups()[2]
            reply = reply.replace( '<EOF>', '' )    # remove Xiva EOF mark
        else:
            self.Log( 'Xiva problem: %s' % resp )
        return (ok, reply)
        
    def GetDisc( self, aMeta, aNumDiscs, aNumEntry ):                 
        "Get information for a 'disc' from Xiva"
        m = re.search( 
            '(\d+)<NAME>(.*)<ID>(\d+)<ARTIST>(.*)<GENRE>(.*)$', aMeta )
        media = {'id':     m.groups()[2],
                 'artist': m.groups()[3],
                 'album':  m.groups()[1],
                 'genre':  m.groups()[4].replace( 'General ', '' )}
        
        (ok, resp) = self.XivaSend( 'SEARCH', '<MEDIA><ID>%s' % media['id'] )
        m = re.search( '(.*)<TOTAL>(\d+)<SOURCE>', resp )
        count = int( m.groups()[1] )
        
        media['tracks'] = self.GetTracks( media['id'], count )
        self.Log( '    %d/%d  %s by %s  %d tracks' % 
            (aNumEntry, aNumDiscs, media['album'], media['artist'], count) )
        return media
                
    def GetTracks( self, aId, aCount ):
        "Get information for all tracks on disk with specified ID from Xiva"
        next   = 1
        tracks = []
        while next <= aCount:
            req = '<MEDIA><ID>%s<TRACK><FROM>%s<FOR>99' % (aId, next)
            (ok, resp) = self.XivaSend( 'SEARCH', req )
            m = re.search( '(.*)<FROM>(\d+)<FOR>(\d+)<AT>(.*)', resp )
            next += int( m.groups()[2] )
            data = m.groups()[3]
            
            entries = data.split( '<AT>' )
            for entry in entries:
                m = re.search( '(\d+)<ID>(.*)<NAME>(.*)$', entry )
                track = { 'number': m.groups()[0],
                          'title':  m.groups()[2] }
                tracks.append( track )
        return tracks
        
    def Filesafe( self, aStr ):
        "Convert string to format safe to be used within filename"
        safe = aStr.replace( '\\', '-' )
        safe = safe.replace( ':', '-' )
        safe = safe.replace( '/', '-' )
        safe = safe.replace( '*', '-' )
        safe = safe.replace( '?', '' )
        safe = safe.replace( '"', "'" )
        safe = safe.replace( '<', '-' )
        safe = safe.replace( '>', '-' )
        safe = safe.replace( '|', '-' )
        safe = safe.replace( '`', "'" )
        safe = safe.replace( '$', "S" )
        safe = safe.strip( '.' )
        safe = safe.strip()
        return safe
    
    def Tagsafe( self, aStr ):
        "Convert string to format safe to be used within tags"
        safe = aStr.replace( '"', "'" )
        safe = safe.replace( '`', "'" )
        return safe
    
    def CheckWritePerms( self, aPath ):
        "Check can write file to specified path"
        fName = os.path.normpath( aPath + '/' + 'tESTfILE.tXT' )
        ok    = True
        try:
            f = open( fName, 'wt' )
            f.write( 'Test Message' )
            f.close()
            os.unlink( fName )
        except:
            ok = False
        return ok
        
    def MainIsFrozen( self ):
        "True if running EXE, False if running PY"
        return( hasattr( sys, "frozen" ) or       # new py2exe
                hasattr( sys, "importers" )       # old py2exe
                or imp.is_frozen( "__main__" ))   # tools/freeze
   
    def GetMainDir( self ):
        "Return CWD for script or exe"
        if self.MainIsFrozen():
            return os.path.normpath( os.path.dirname( sys.executable ))
        return os.path.normpath( os.path.dirname( sys.argv[0] ))

    def ShowUsage( self ):
        "Display help page"
        self.Log( 'KivorExport <kivorip> <naspath>' )
        self.Log( '    <kivorip> is IP address of Kivor in form nnn.nnn.nnn.nnn' )
        self.Log( '    <naspath> is path of root folder on NAS to contain the files' )
        self.Log( '        - they will be stored here in an Artist/Album/Track structure' )
        self.Log( '' )
        
    def ShowStats( self ):
        "Display statistics"
        localPath = self.GetMainDir()
        self.Log( '' )
        self.Log( 'Transferred %d tracks from %d albums:' % \
                  (self.stats['tracks'], self.stats['discs']) )
        self.Log( '    % 5d succesful' % self.stats['xfer'] )  
        self.Log( '    % 5d skipped' % self.stats['skip'] )  
        self.Log( '    % 5d failed' % self.stats['fail'] )  
        self.Log( '' )
        self.Log( 'Transfer logged: %s' % os.path.normpath( localPath+'/KivorExport.txt' ))
        self.Log( 'Failures logged: %s' % os.path.normpath( localPath+'/kivorExportFAILS.txt' ))
        self.Log( 'Completed' )
        self.Log( '' )
    
    def Die( self, aMsg ):
        "Log message and exit script"
        self.Log( '' )
        self.Log( aMsg )
        self.Log( 'ABORTING' )
        self.Log( '' )
        self.Shutdown()

    def Fail( self, aDisc, aTrack, aSrc, aErr ):
        "Write failure info to fails file"
        msg = 'Disc: %s by %s, Track: %s - %s' % \
            ( aDisc['album'], aDisc['artist'], aTrack['number'], aTrack['title'] )
        self.failsfile.write( msg + '\r\n' )
        self.failsfile.write( '   Kivor source: %s\r\n' % aSrc )
        self.failsfile.write( '   Error message: %s\r\n\r\n' % aErr )
        self.failsfile.flush()
        self.Log( '           -> Kivor source:  %s' % aSrc )
        self.Log( '           -> Error message: %s' % aErr )
        self.Log( '           **** FAILED ****' )

    def Log( self, aMsg ):
        "Log a single-line progress message with date/time stamp"
        timestamp = time.strftime('%y-%m-%d %H:%M:%S', time.localtime( time.time() ))
        msg       = '%s> %s' % (timestamp, aMsg )
        self.logfile.write( msg + '\r\n' )
        self.logfile.flush()
        print msg
        

if __name__ == '__main__':        

    k = KivorExport( sys.argv )
