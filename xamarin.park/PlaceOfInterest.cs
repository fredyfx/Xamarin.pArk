using MonoTouch.UIKit;
using MonoTouch.CoreLocation;

namespace Xamarin.pArk
{
    public class PlaceOfInterest
    {
        public int Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public UIView View 
        {
            get;
            set;
        }

        public CLLocation Location 
        {
            get;
            set;
        }
    }
}

