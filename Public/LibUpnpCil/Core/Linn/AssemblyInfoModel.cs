using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Linn
{
	internal class AssemblyInfoModel
	{
		private string iProduct;
		internal string Product
        {
            get
            {
            	return iProduct;
            }
        }
		
		private string iTitle;
		internal string Title
        {
            get
            {
                return iTitle;
            }
        }
		
		private string iCopyright;
		internal string Copyright
        {
            get
            {
            	return iCopyright;
            }
        }
		
		private string iCompany;
		internal string Company
        {
            get
            {
                return iCompany;
            }
        }
		
		private string iVersion;
		internal string Version
        {
            get
            {
                return iVersion;
            }
        }
		
		private string iDescription;
		internal string Description
        {
            get
            {
            	return iDescription;
            }
        }

        private string iInformationalVersion;
        internal string InformationalVersion
        {
            get
            {
                return iInformationalVersion;
            }
        }
		
		public AssemblyInfoModel(string aDescription
		                                    ,string aVersion
		                                    ,string aCompany
		                                    ,string aCopyright
                                            , string aTitle
                                            , string aProduct
                                            , string aInformationalVersion)
        {
			iDescription = aDescription;
			iVersion = aVersion;
			iCompany = aCompany;
			iCopyright = aCopyright;
			iTitle = aTitle;
			iProduct = aProduct;
            iInformationalVersion = aInformationalVersion;
		}
        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}, {3}, {4}, {5}", iDescription, iVersion, iCompany, iCopyright, iTitle, iProduct);
        }
	}
}