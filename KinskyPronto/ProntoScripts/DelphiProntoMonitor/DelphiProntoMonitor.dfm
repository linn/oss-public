object Form1: TForm1
  Left = 188
  Top = 189
  Width = 870
  Height = 500
  Caption = 'Philips Pronto TSU 9x00 Monitor'
  Color = clBtnFace
  Font.Charset = ANSI_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'Tahoma'
  Font.Style = []
  OldCreateOrder = False
  OnClose = FormClose
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object GroupBox1: TGroupBox
    Left = 8
    Top = 8
    Width = 849
    Height = 457
    Anchors = [akLeft, akTop, akRight, akBottom]
    Caption = 'Pronto Output'
    TabOrder = 0
    object Memo2: TMemo
      Left = 8
      Top = 16
      Width = 833
      Height = 433
      Anchors = [akLeft, akTop, akRight, akBottom]
      Lines.Strings = (
        '')
      ScrollBars = ssBoth
      TabOrder = 0
    end
  end
  object ServerSocket1: TServerSocket
    Active = False
    Port = 0
    ServerType = stNonBlocking
    OnClientRead = ServerSocket1ClientRead
    Left = 400
    Top = 328
  end
end
