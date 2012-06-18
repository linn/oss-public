program ProntoMonitor;

uses
  Forms,
  DelphiProntoMonitor in 'DelphiProntoMonitor.pas' {Form1};

{$R *.RES}

begin
  Application.Initialize;
  Application.Title := 'Philips Pronto TSU 9x00 Monitor';
  Application.CreateForm(TForm1, Form1);
  Application.Run;
end.
