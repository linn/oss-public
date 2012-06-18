<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" doctype-public="-//W3C//DTD XHTML 1.1//EN"/> 
	<xsl:param name="bg"></xsl:param>
	<xsl:param name="fg"></xsl:param>
    <xsl:param name="hibg"></xsl:param>
    <xsl:param name="hifg"></xsl:param>
    <xsl:template match="/">
        <html>
	        <head>
		        <title><xsl:value-of select="/station/title"/></title>
                <style type="text/css">
                    body {
                      background-color: <xsl:value-of select="$bg"/>;
                      color: <xsl:value-of select="$fg"/>;
                      font-family: Sans-serif
                      font-size: 12px
                    }
                    h1 {
                      font-size: 16px
                    }
                </style>
            </head>
	        <body>
                <xsl:element name="a">
                    <xsl:attribute name="href">
                        <xsl:value-of select="/station/link"/>
                    </xsl:attribute>
                    <xsl:element name="img">
                        <xsl:attribute name="src">
                            <xsl:value-of select="/station/logo"/>
                        </xsl:attribute>
                    </xsl:element>
                </xsl:element>
	    	    <h1>
			        <xsl:value-of select="/station/title"/>
	    	    </h1>
                <p>
                    <xsl:value-of select="/station/country"/>
                    <br/>
                    <xsl:value-of select="/station/region"/>
                    <br/>
                    <xsl:value-of select="/station/bitrate"/>kbps
                    <br/>
                </p>
                <p>
                    <xsl:value-of select="/station/description"/>
                </p>
		    </body>
        </html>
    </xsl:template>
</xsl:stylesheet>
