unit DelphiProntoMonitor;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, ScktComp, WinSock;

type
  TForm1 = class(TForm)
    ServerSocket1: TServerSocket;
    GroupBox1: TGroupBox;
    Memo2: TMemo;
    procedure FormCreate(Sender: TObject);
    procedure ServerSocket1ClientRead(Sender: TObject;
      Socket: TCustomWinSocket);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  Form1: TForm1;

implementation

var
  lastClient: string;

{$R *.DFM}



function getLocalIP: string;

var
  wsaData: TWSAData;
  addr: TSockAddrIn;
  Phe: PHostEnt;
  szHostName: array[0..128] of Char;

begin

  Result := '';

  if WSAStartup($101, WSAData) <> 0 then begin
    ShowMessage('Unable to initialise WinSock.');
    Exit;
  end;

  try
    if GetHostName(szHostName, 128) <> SOCKET_ERROR then begin
      Phe := GetHostByName(szHostName);
      if Assigned(Phe) then begin
        addr.sin_addr.S_addr := longint(plongint(Phe^.h_addr_list^)^);
        Result := inet_ntoa(addr.sin_addr);
      end;
    end;
  finally
    WSACleanup;
  end;

end; {getLocalIP}

procedure TForm1.FormCreate(Sender: TObject);
begin
  lastClient := '';
  ServerSocket1.Port := 23;
  ServerSocket1.Active := True;
  GroupBox1.Caption := 'Pronto Output: [This machine is '+getLocalIP()+']'

end; {TForm1.FormCreate}

procedure TForm1.ServerSocket1ClientRead(Sender: TObject;
  Socket: TCustomWinSocket);
var
  i:integer;
  sRec : string;
begin
  for i := 0 to ServerSocket1.Socket.ActiveConnections-1 do begin
    with ServerSocket1.Socket.Connections[i] do begin
      sRec := ReceiveText;
      if (sRec <> '') then begin
        if (RemoteAddress <> lastClient) then begin
          lastClient := RemoteAddress;
          Memo2.Lines.Add(RemoteAddress + ' sends :') ;
        end; {if}
        Memo2.Lines.Add(sRec);
      end;
    end;
  end;
end {TForm1.ServerSocket1ClientRead};

procedure TForm1.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  ServerSocket1.Active := false;
end;

end.
