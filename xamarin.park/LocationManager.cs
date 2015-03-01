using System;
using MonoTouch.CoreLocation;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace Xamarin.pArk
{
    public class LocationManager
    {
        public event EventHandler<LocationUpdatedEventArgs> LocationUpdated = delegate { };

        public CLLocationManager LocManager
        {
            get;
            private set;
        }

        public LocationManager() : this(new CLLocationManager())
        {
        }

        public LocationManager(CLLocationManager locationManager)
        {
            LocManager = locationManager;
            LocationUpdated += PrintLocation;
        }

        public void StartLocationUpdates()
        {
            // We need the user's permission for our app to use the GPS in iOS. This is done either by the user accepting
            // the popover when the app is first launched, or by changing the permissions for the app in Settings

            if (CLLocationManager.LocationServicesEnabled)
            {

                LocManager.DesiredAccuracy = 1; // sets the accuracy that we want in meters

                // Location updates are handled differently pre-iOS 6. If we want to support older versions of iOS,
                // we want to do perform this check and let our LocationManager know how to handle location updates.
                if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
                {
                    LocManager.RequestWhenInUseAuthorization ();
                    LocManager.LocationsUpdated += (object sender, CLLocationsUpdatedEventArgs e) =>
                    {
                        // fire our custom Location Updated event
                        this.LocationUpdated(this, new LocationUpdatedEventArgs(e.Locations[e.Locations.Length - 1]));
                    };
                }
                else
                {

                    // this won't be called on iOS 6 (deprecated). We will get a warning here when we build.
                    LocManager.UpdatedLocation += (object sender, CLLocationUpdatedEventArgs e) =>
                    {
                        this.LocationUpdated(this, new LocationUpdatedEventArgs(e.NewLocation));
                    };
                }

                // Start our location updates
                LocManager.StartUpdatingLocation();

                // Get some output from our manager in case of failure
                LocManager.Failed += (object sender, NSErrorEventArgs e) =>
                {
                    Console.WriteLine(e.Error);
                }; 
            }
            else
            {
                //Let the user know that they need to enable LocationServices
                Console.WriteLine("Location services not enabled, please enable this in your Settings");
            }
        }

        //This will keep going in the background and the foreground
        public void PrintLocation(object sender, LocationUpdatedEventArgs e)
        {
            CLLocation location = e.Location;

            Console.WriteLine("Altitude: " + location.Altitude + " meters");
            Console.WriteLine("Longitude: " + location.Coordinate.Longitude);
            Console.WriteLine("Latitude: " + location.Coordinate.Latitude);
            Console.WriteLine("Course: " + location.Course);
            Console.WriteLine("Speed: " + location.Speed);
        }
    }
}

