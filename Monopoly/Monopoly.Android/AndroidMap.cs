using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.Gms.Common;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;


namespace Monopoly.Droid {
    public class AndroidMap : Android.Gms.Location.LocationCallback, IMap {

        public event EventHandler<(double, double)> LocationChanged;

        private static Android.Gms.Location.FusedLocationProviderClient fusedLocationProviderClient;

        public static AndroidMap Instance { get; protected set; }

        static AndroidMap() {
            Instance = new AndroidMap();
        }

        private AndroidMap() {
            if (!IsGooglePlayServicesInstalled()) {
                throw new Exception("Google Play services not installed.");
            }

            // Get GPS provider
            fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity);
            
           
            
            var locationRequest = new LocationRequest()
                                        .SetInterval(2000)
                                        .SetFastestInterval(1999)
                                        .SetPriority(LocationRequest.PriorityHighAccuracy);
            fusedLocationProviderClient.RequestLocationUpdatesAsync(locationRequest, this);
            
        }

        private bool IsGooglePlayServicesInstalled() {
            var queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity);

            if (queryResult == ConnectionResult.Success) {
                //Log.Info("MainActivity", "Google Play Services is installed on this device.");
                return true;
            }

            if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult)) {
                // Check if there is a way the user can resolve the issue
                var errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
                //Log.Error("MainActivity", "There is a problem with Google Play Services on this device: {0} - {1}",
                          //queryResult, errorString);

                // Alternately, display the error to the user.
            }

            return false;
        }

        public async Task<(double, double)> GetCurrentCoordinates() {
            var location = await fusedLocationProviderClient.GetLastLocationAsync();
            if (location == null)
                return (0, 0);
            System.Diagnostics.Debug.WriteLine($"Acquired (latitude, longitude) = ({location.Latitude},{location.Longitude})");
            return (location.Latitude, location.Longitude);
        }

        public override void OnLocationAvailability(LocationAvailability locationAvailability) {
            System.Diagnostics.Debug.WriteLine($"Location Availability: {locationAvailability.IsLocationAvailable}");
        }

        public override void OnLocationResult(LocationResult result) {
            if (result.Locations.Any()) {
                var location = result.Locations.First();
                LocationChanged(this, (location.Latitude, location.Longitude));
            }
            else {
                System.Diagnostics.Debug.WriteLine("No locations to work with.");
            }
        }
    }
}