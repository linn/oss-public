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
                if ((aModel == ProductSupport.kModelKlimaxDs) ||
                    (aModel == ProductSupport.kModelKlimaxDsm)) {
                    imgKey = kKlimaxDsKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.KlimaxDsIcon;
                }
                else if (aModel == ProductSupport.kModelRenewDs) {
                    imgKey = kRenewDsKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.RenewDsIcon;
                }
                else if (aModel == ProductSupport.kModelAkurateDs && aIsNew) { 
                    imgKey = kAkurateDs2010Key; 
                    img = Linn.Toolkit.WinForms.Properties.Resources.AkurateDs2010Icon; 
                }
                else if ((aModel == ProductSupport.kModelAkurateDs) ||
                         (aModel == ProductSupport.kModelMajikDs) ||
                         (aModel == ProductSupport.kModelConceptDs)) {
                    imgKey = kDsKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.AkurateDsIcon;
                }
                else if (aModel == ProductSupport.kModelSneakyMusicDs) {
                    imgKey = kSneakyMusicDsKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.SneakyMusicDsIcon;
                }
                else if (aModel == ProductSupport.kModelSekritDsi) {
                    imgKey = kSekritDsiKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.SekritDsiIcon;
                }
                else if ((aModel == ProductSupport.kModelMajikDsi) ||
                         (aModel == ProductSupport.kModelMajikDsm)) {
                    imgKey = kMajikDsiKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.MajikDsiIcon;
                }
                // need to diferentiate between volkano Akuarte Kontrol and proxy Akurate Kontrol
                else if ((aModel == ProductSupport.kModelAkurateKontrol && aType != EDeviceType.eProxy) ||
                         (aModel == ProductSupport.kModelAkurateDsm)) {
                    imgKey = kAkurateKontrol2010Key; 
                    img = Linn.Toolkit.WinForms.Properties.Resources.AkurateKontrol2010Icon;
                }
                else if ((aModel == ProductSupport.kModelAkurateKontrol) ||
                         (aModel == ProductSupport.kModelKinos) ||
                         (aModel == ProductSupport.kModelMajikKontrol) ||
                         (aModel == ProductSupport.kModelMajikI)) {
                    imgKey = kKontrolKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.AkurateKontrolIcon;
                }
                else if (aModel == ProductSupport.kModelKisto) {
                    imgKey = kKistoKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.KistoIcon;
                }
                else if (aModel == ProductSupport.kModelKlimaxKontrol) {
                    imgKey = kKlimaxKontrolKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.KlimaxKontrolIcon;
                }
                else if (aModel == ProductSupport.kModelRoomAmp2) {
                    imgKey = kRoomAmp2Key;
                    img = Linn.Toolkit.WinForms.Properties.Resources.RoomAmp2Icon;
                }
                else if (aModel == ProductSupport.kModelCd12) {
                    imgKey = kCd12Key;
                    img = Linn.Toolkit.WinForms.Properties.Resources.Cd12Icon;
                }
                else if ((aModel == ProductSupport.kModelUnidiskSC) ||
                         (aModel == ProductSupport.kModelAkurateCd) ||
                         (aModel == ProductSupport.kModelUnidisk1_1) ||
                         (aModel == ProductSupport.kModelUnidisk2_1) ||
                         (aModel == ProductSupport.kModelMajikCd)) {
                    imgKey = kDiscKey;
                    img = Linn.Toolkit.WinForms.Properties.Resources.AkurateCdIcon;
                }
                else if ((aModel == ProductSupport.kModelClassikMovie) ||
                         (aModel == ProductSupport.kModelClassikMusic)) {
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
        private static string kSekritDsiKey = "kSekritDsi";
        private static string kMajikDsiKey = "kMajikDsi";
        private static string kFallbackKey = "Fallback";
        private static string kMediaServerKey = "MediaServer";
        private static string kRoomKey = "Room";

        static private ProductImages iInstance = null;
    }
} // Linn