using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Android;
using Android.Gms.Location;
using Android.Content;
using Android.Gms.Maps;
using Xamarin.Forms.Internals;
using Android.Gms.Maps.Model;
using System.Threading.Tasks;

[assembly: ExportRenderer(typeof(Monopoly.Droid.CustomMap), typeof(Monopoly.Droid.CustomMapRenderer))]
namespace Monopoly.Droid {
    public class CustomMapRenderer : MapRenderer {
        
        Dictionary<Polygon, LocationStats> polytoLocation;
        private Game game;
        private Task<List<LocationStats>> location_task;

        public CustomMapRenderer(Context context) : base(context) {
            game = Monopoly.Game.Instance;
            location_task = game.GetAllLocations();

        }

        protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<Map> e) {
            base.OnElementChanged(e);

            if (e.OldElement != null) {
                // Unsubscribe
            }

            if (e.NewElement != null) {
                var formsMap = (Monopoly.Droid.CustomMap)e.NewElement;
                this.game = formsMap.game;
                Control.GetMapAsync(this);
            }

        }

        protected override void OnMapReady(Android.Gms.Maps.GoogleMap map) {
            location_task.Wait();
            var allLocations = location_task.Result;
            polytoLocation = new Dictionary<Polygon, LocationStats>();
            foreach(LocationStats x in allLocations)
            {
                PolygonOptions y = new PolygonOptions();
                if (x.Owner == null)
                {
                    y.InvokeFillColor(0x808080);
                    y.Clickable(true);
                }
                else if (x.Owner == this.game.Player.Name)
                {
                    y.InvokeFillColor(0x0000ff);
                }
                else
                {
                    y.InvokeFillColor(0x800080);
                }

                foreach(var position in getEdges(x.Corners.Item1, x.Corners.Item2))
                {
                    y.Add(new LatLng(position.Latitude, position.Longitude));
                }
                var poly = NativeMap.AddPolygon(y);
                polytoLocation.Add(poly, x);
            }
            NativeMap.PolygonClick += NativeMap_PolygonClick;
            
        }
        
        private async void NativeMap_PolygonClick(object sender, GoogleMap.PolygonClickEventArgs e)
        {
            var NativeItem = e.Polygon;
            LocationStats loc = polytoLocation[NativeItem];
            IMap x = null;
#if __ANDROID__
            x = Monopoly.Droid.AndroidMap.Instance;
#endif
            var current = await x.GetCurrentCoordinates();
            if (loc.Contains(current))
            {
                if (loc.Owner == null)
                    await this.game.Purchase(new Location(loc, 1));
            }
            else
            {
                return;
            }
        }

        private static List<Position> getEdges((double,double) NE, (double,double) SW)
        {
            Position Northeast = new Position(NE.Item1, NE.Item2);
            Position Southwest = new Position(SW.Item1, SW.Item2);

            Position Southeast = new Position(Southwest.Latitude, Northeast.Longitude);
            Position Northwest = new Position(Northeast.Latitude, Southwest.Longitude);
            List<Position> PolygonList = new List<Position> { Northeast, Southeast, Southwest, Northwest };
            return PolygonList;
        }
    }
}
