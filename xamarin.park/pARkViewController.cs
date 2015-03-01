using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.CoreLocation;

namespace Xamarin.pArk
{
    public class pARkViewController : UIViewController
    {
        LocationManager LocationManager;
        ARView _arView;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            
            // Perform any additional setup after loading the view, typically from a nib.
            LocationManager = new LocationManager();
            View = _arView = new ARView();

            var placesOfInterest = new List<PlaceOfInterest>();

            placesOfInterest.Add(new PlaceOfInterest
            {
                Id = 0,
                Name = "Central Park NY",
                Location = new CLLocation(new CLLocationCoordinate2D(40.7711329, -73.9741874), 0.0, 0, 0, NSDate.Now),
            });

            placesOfInterest.Add(new PlaceOfInterest
            {
                Id = 1,
                Name = "Golden Gate Bridge",
                Location = new CLLocation(new CLLocationCoordinate2D(37.8197, -122.4786), 0.0, 0, 0, NSDate.Now),
            });

            placesOfInterest.Add(new PlaceOfInterest
            {
                Id = 2,
                Name = "Pluralsight Office",
                Location = new CLLocation(new CLLocationCoordinate2D(40.984112, -111.908368), 0.0, 0, 0, NSDate.Now),
            });

            placesOfInterest.Add(new PlaceOfInterest
            {
                Id = 3,
                Name = "Home",
                Location = new CLLocation(new CLLocationCoordinate2D(33.561441, -82.066884), 0.0, 0, 0, NSDate.Now),
            });

            _arView.PlacesOfInterest = placesOfInterest;

            UIApplication.Notifications.ObserveDidBecomeActive((sender, args) =>
            {
                LocationManager.LocationUpdated += HandleLocationUpdated;
            });

            UIApplication.Notifications.ObserveDidEnterBackground((sender, args) =>
            {
                LocationManager.LocationUpdated -= HandleLocationUpdated;
            });

            LocationManager.StartLocationUpdates();

            _arView.Start();
        }

        void HandleLocationUpdated(object sender, LocationUpdatedEventArgs e)
        {
            _arView.UpdatePlacesOfInterestCoordinates(e.Location);
        }
                        
        public override void ViewDidDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            _arView.Stop();
        }
    }
}
