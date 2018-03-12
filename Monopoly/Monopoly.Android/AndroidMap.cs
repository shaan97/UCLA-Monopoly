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
    class AndroidMap : IMap {

        public event EventHandler<((double, double), (double, double))> LocationChanged;

        private static Android.Gms.Location.FusedLocationProviderClient fusedLocationProviderClient;

        public AndroidMap() {
            // TODO : Does this cast fail?
            fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient((Android.App.Activity)Plugin.CurrentActivity.CrossCurrentActivity.Current);
            
            /*
            
            var locationRequest = new LocationRequest()
                                        .SetInterval(60 * 1000 * 5)
                                        .SetFastestInterval(60 * 1000 * 1)
                                        .SetPriority(LocationRequest.PriorityHighAccuracy);
            await fusedLocationProviderClient.RequestLocationUpdatesAsync(locationRequest, this);
            
            */
        }

        public async Task<(double, double)> GetCurrentCoordinates() {
            var location = await fusedLocationProviderClient.GetLastLocationAsync();
            if (location == null)
                return (0, 0);
            System.Diagnostics.Debug.WriteLine($"Acquired (latitude, longitude) = ({location.Latitude},{location.Longitude})");
            return (location.Latitude, location.Longitude);
        }
    }
}