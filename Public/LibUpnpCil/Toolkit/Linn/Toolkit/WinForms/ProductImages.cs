using System.Drawing;
using System.Windows.Forms;

namespace Linn.Toolkit.WinForms
{
    public class ProductImages
    {
        public static ProductImages Instance {
            get {
                if (iInstance == null) {
                    iInstance = new ProductImages();
                }
                return iInstance;
            }
        }

        public static Image GetImage(EDeviceType aType, string aModel, bool aIsNew)
        {
            string temp = "";
            return ProductImages.Instance.GetImageInfo(aType, aModel, aIsNew, out temp);
        }

        public static Image GetImage(EDeviceType aType, string aModel, bool aIsNew, Size aSize)
        {
            string temp = "";
            ImageList imageList = new ImageList();
            imageList.ImageSize = aSize;
#if !PocketPC
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            imageList.TransparentColor = Color.Transparent;
#endif
            imageList.Images.Add(ProductImages.Instance.GetImageInfo(aType, aModel, aIsNew, out temp));
            return imageList.Images[0];
        }

        public static string GetKey(EDeviceType aType, string aModel, bool aIsNew)
        {
            string key = "";
            ProductImages.Instance.GetImageInfo(aType, aModel, aIsNew, out key);
            return key;
        }

        public static ImageList CreateImageListSmall
        {
            get
            {
                return ProductImages.Instance.CreateImageList(new Size(40, 18));
            }
        }

        public static ImageList CreateImageListMedium
        {
            get
            {
                return ProductImages.Instance.CreateImageList(new Size(60, 25));
            }
        }

        public static ImageList CreateImageListLarge
        {
            get
            {
                return ProductImages.Instance.CreateImageList(new Size(120, 50));
            }
        }

        private Image GetImageInfo(EDeviceType aType, string aModel, bool aIsNew, out string aKey)
        {
            string imgKey = kDefaultKey;
            Image img = Linn.Toolkit.WinForms.Properties.Resources.DefaultIcon;

            if (aType == EDeviceType.eFallback)
            {
                imgKey = kFallbackKey;
                img = Linn.Toolkit.WinForms.Properties.Resources.FallbackIcon;
            }
            else if (aType == EDeviceType.eMediaServer)
            {
                imgKey = kMediaServerKey;
                img = Linn.Toolkit.WinForms.Properties.Resources.MediaServerIcon;
            }
            else if (aType == EDeviceType.eNone)
            {
                imgKey = kRoomKey;
                img = Linn.Toolkit.WinForms.Properties.Resources.RoomIcon;
            }
            else
            {
                if ((aModel == ModelInfo.kModelKlimaxDs) ||
                    (aModel == ModelInfo.kModelKlimaxDsm)) {
                    imgKey = kKlimaxDsKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.KlimaxDsIcon;
                }
                else if (aModel == ModelInfo.kModelRenewDs) {
                    imgKey = kRenewDsKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.RenewDsIcon;
                }
                else if (aModel == ModelInfo.kModelAkurateDs && aIsNew) { 
                    imgKey = kAkurateDs2010Key; 
                    img = Linn.Toolkit.WinForms.Properties.Resources.AkurateDs2010Icon; 
                }
                else if ((aModel == ModelInfo.kModelAkurateDs) ||
                         (aModel == ModelInfo.kModelMajikDs) ||
                         (aModel == ModelInfo.kModelConceptDs)) {
                    imgKey = kDsKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.AkurateDsIcon;
                }
                else if (aModel == ModelInfo.kModelSneakyMusicDs) {
                    imgKey = kSneakyMusicDsKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.SneakyMusicDsIcon;
                }
                else if (aModel == ModelInfo.kModelSekritDsi) {
                    imgKey = kSekritDsiKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.SekritDsiIcon;
                }
                else if (aModel == ModelInfo.kModelMajikDsm && aIsNew) {
                    imgKey = kMajikDsmKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.MajikDsmIcon;
                }
                else if (aModel == ModelInfo.kModelKikoDsm) {
                    imgKey = kKikoDsmKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.KikoDsmIcon;
                }
                else if ((aModel == ModelInfo.kModelMajikDsi) ||
                         (aModel == ModelInfo.kModelMajikDsm)) {
                    imgKey = kMajikDsiKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.MajikDsiIcon;
                }
                // need to diferentiate between volkano Akuarte Kontrol and proxy Akurate Kontrol
                else if ((aModel == ModelInfo.kModelAkurateKontrol && aType != EDeviceType.eProxy) ||
                         (aModel == ModelInfo.kModelAkurateDsm)) {
                    imgKey = kAkurateKontrol2010Key; 
                    img = Linn.Toolkit.WinForms.Properties.Resources.AkurateKontrol2010Icon;
                }
                else if ((aModel == ModelInfo.kModelAkurateKontrol) ||
                         (aModel == ModelInfo.kModelKinos) ||
                         (aModel == ModelInfo.kModelMajikKontrol) ||
                         (aModel == ModelInfo.kModelMajikI)) {
                    imgKey = kKontrolKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.AkurateKontrolIcon;
                }
                else if (aModel == ModelInfo.kModelKisto) {
                    imgKey = kKistoKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.KistoIcon;
                }
                else if (aModel == ModelInfo.kModelKlimaxKontrol) {
                    imgKey = kKlimaxKontrolKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.KlimaxKontrolIcon;
                }
                else if (aModel == ModelInfo.kModelRoomAmp2) {
                    imgKey = kRoomAmp2Key;
                    img = Linn.Toolkit.WinForms.Properties.Resources.RoomAmp2Icon;
                }
                else if (aModel == ModelInfo.kModelCd12) {
                    imgKey = kCd12Key;
                    img = Linn.Toolkit.WinForms.Properties.Resources.Cd12Icon;
                }
                else if ((aModel == ModelInfo.kModelUnidiskSC) ||
                         (aModel == ModelInfo.kModelAkurateCd) ||
                         (aModel == ModelInfo.kModelUnidisk1_1) ||
                         (aModel == ModelInfo.kModelUnidisk2_1) ||
                         (aModel == ModelInfo.kModelMajikCd)) {
                    imgKey = kDiscKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.AkurateCdIcon;
                }
                else if ((aModel == ModelInfo.kModelClassikMovie) ||
                         (aModel == ModelInfo.kModelClassikMusic)) {
                    imgKey = kClassikKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.ClassikIcon;
                }
            }
            aKey = imgKey;
            return img;
        }

        private ImageList CreateImageList(Size aSize)
        {
#if PocketPC
                return null;
#else
            ImageList imageList = new ImageList();
            imageList.ImageSize = aSize;
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            imageList.TransparentColor = Color.Transparent;
            imageList.Images.Add(kDefaultKey, Linn.Toolkit.WinForms.Properties.Resources.DefaultIcon);
            imageList.Images.Add(kDsKey, Linn.Toolkit.WinForms.Properties.Resources.AkurateDsIcon);
            imageList.Images.Add(kKontrolKey, Linn.Toolkit.WinForms.Properties.Resources.AkurateKontrolIcon);
            imageList.Images.Add(kDiscKey, Linn.Toolkit.WinForms.Properties.Resources.AkurateCdIcon);
            imageList.Images.Add(kCd12Key, Linn.Toolkit.WinForms.Properties.Resources.Cd12Icon);
            imageList.Images.Add(kClassikKey, Linn.Toolkit.WinForms.Properties.Resources.ClassikIcon);
            imageList.Images.Add(kKistoKey, Linn.Toolkit.WinForms.Properties.Resources.KistoIcon);
            imageList.Images.Add(kKlimaxDsKey, Linn.Toolkit.WinForms.Properties.Resources.KlimaxDsIcon);
            imageList.Images.Add(kRenewDsKey, Linn.Toolkit.WinForms.Properties.Resources.RenewDsIcon);
            imageList.Images.Add(kAkurateDs2010Key, Linn.Toolkit.WinForms.Properties.Resources.AkurateDs2010Icon); 
            imageList.Images.Add(kAkurateKontrol2010Key, Linn.Toolkit.WinForms.Properties.Resources.AkurateKontrol2010Icon);
            imageList.Images.Add(kKlimaxKontrolKey, Linn.Toolkit.WinForms.Properties.Resources.KlimaxKontrolIcon);
            imageList.Images.Add(kRoomAmp2Key, Linn.Toolkit.WinForms.Properties.Resources.RoomAmp2Icon);
            imageList.Images.Add(kSneakyMusicDsKey, Linn.Toolkit.WinForms.Properties.Resources.SneakyMusicDsIcon);
            imageList.Images.Add(kSekritDsiKey, Linn.Toolkit.WinForms.Properties.Resources.SekritDsiIcon);
            imageList.Images.Add(kMajikDsiKey, Linn.Toolkit.WinForms.Properties.Resources.MajikDsiIcon);
            imageList.Images.Add(kMajikDsmKey, Linn.Toolkit.WinForms.Properties.Resources.MajikDsmIcon);
            imageList.Images.Add(kKikoDsmKey, Linn.Toolkit.WinForms.Properties.Resources.KikoDsmIcon);
            imageList.Images.Add(kFallbackKey, Linn.Toolkit.WinForms.Properties.Resources.FallbackIcon);
            imageList.Images.Add(kMediaServerKey, Linn.Toolkit.WinForms.Properties.Resources.MediaServerIcon);
            imageList.Images.Add(kRoomKey, Linn.Toolkit.WinForms.Properties.Resources.RoomIcon);
            return imageList;
#endif
        }

        private static string kDefaultKey = "Default";
        private static string kDsKey = "Ds";
        private static string kKontrolKey = "Kontrol";
        private static string kDiscKey = "Disc";
        private static string kCd12Key = "Cd12";
        private static string kClassikKey = "Classik";
        private static string kKistoKey = "Kisto";
        private static string kKlimaxDsKey = "KlimaxDs";
        private static string kRenewDsKey = "RenewDs";
        private static string kAkurateDs2010Key = "AkurateDs2010"; 
        private static string kAkurateKontrol2010Key = "AkurateKontrol2010";
        private static string kKlimaxKontrolKey = "KlimaxKontrol";
        private static string kRoomAmp2Key = "RoomAmp2";
        private static string kSneakyMusicDsKey = "SneakyMusicDs";
        private static string kSekritDsiKey = "SekritDsi";
        private static string kMajikDsiKey = "MajikDsi";
        private static string kMajikDsmKey = "MajikDsm";
        private static string kKikoDsmKey = "KikoDsm";
        private static string kFallbackKey = "Fallback";
        private static string kMediaServerKey = "MediaServer";
        private static string kRoomKey = "Room";

        static private ProductImages iInstance = null;
    }

    public class ModelInfo
    {
        public const string kModelKlimaxDs = "Klimax DS";
        public const string kModelKlimaxDsm = "Klimax DSM";
        public const string kModelAkurateDs = "Akurate DS";
        public const string kModelAkurateDsm = "Akurate DSM";
        public const string kModelMajikDs = "Majik DS";
        public const string kModelSneakyMusicDs = "Sneaky Music DS";
        public const string kModelSekritDsi = "Sekrit DS-I";
        public const string kModelMajikDsi = "Majik DS-I";
        public const string kModelMajikDsm = "Majik DSM";
        public const string kModelKikoDsm = "Kiko DSM";
        public const string kModelRenewDs = "Renew DS";
        public const string kModelConceptDs = "Concept DS";
        public const string kModelConceptDsm = "Concept DSM";
        public const string kModelProxyNone = "None";
        public const string kModelCd12 = "CD12";
        public const string kModelAkurateCd = "Akurate CD";
        public const string kModelUnidisk1_1 = "Unidisk 1.1";
        public const string kModelUnidisk2_1 = "Unidisk 2.1";
        public const string kModelUnidiskSC = "Unidisk SC";
        public const string kModelMajikCd = "Majik CD";
        public const string kModelClassikMovie = "Classik Movie";
        public const string kModelClassikMusic = "Classik Music";
        public const string kModelKlimaxKontrol = "Klimax Kontrol";
        public const string kModelAkurateKontrol = "Akurate Kontrol";
        public const string kModelKisto = "Kisto";
        public const string kModelKinos = "Kinos";
        public const string kModelMajikKontrol = "Majik Kontrol";
        public const string kModelMajikI = "Majik-I";
        public const string kModelRoomAmp2 = "Roomamp 2";
    }

} // Linn