using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Android;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using Android.Gms.Maps;
using Android.Locations;
using Android.Runtime;
using Android.Views;
using Android.OS;
using Android.Gms.Maps.Model;
using Xamarin.Forms.Internals;
using Java.Lang;
using Math = System.Math;
using LatLng = Android.Gms.Maps.Model.LatLng;
using Android.Util;
using Log = Android.Util.Log;


namespace Monopoly.Droid {
    public class MapPage : ContentPage {
        private Game game;
        private IMap map;
        private Label MoneyLabel;
        private Button InventoryButton;
        private Label InventoryLabel;
        private Grid MapGrid;
        private Grid InventoryGrid;

        public MapPage(Game game)
        {
            this.game = game;
#if __ANDROID__
            this.map = Monopoly.Droid.AndroidMap.Instance;
#endif
            /* LABEL 1: MONEY */
            MoneyLabel = new Label();
            MoneyLabel.VerticalOptions = LayoutOptions.Start;
            MoneyLabel.HorizontalOptions = LayoutOptions.Start;
            var s = new FormattedString();
            s.Spans.Add(new Span { Text = "500", FontAttributes = FontAttributes.Bold, FontSize = 30 });
            MoneyLabel.FormattedText = s;
            this.game.OnCreditsChange += UpdateMoney;

            /* BUTTON 1: INVENTORY */
            InventoryButton = new Button();
            InventoryButton.HorizontalOptions = LayoutOptions.Start;
            InventoryButton.VerticalOptions = LayoutOptions.End;
            InventoryButton.Text = "Inventory";
            InventoryButton.Clicked += InventoryButton_Clicked;

            /* MAP */
            var customMap = new CustomMap(this.game);
            customMap.IsShowingUser = true;
            
            customMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(34.066510, -118.449391), Distance.FromMiles(0.1)));

            MapGrid = new Grid {
                Children = { customMap, MoneyLabel, InventoryButton }
            };
            this.Content = MapGrid;
        }

        private void InventoryButton_Clicked(object sender, EventArgs e) {
            var Locations = this.game.Player.Locations;
            string places = "";
            int place_num = 1;
            foreach (Monopoly.Location x in Locations)
            {
                places += place_num.ToString();
                places += ") ";
                places += x.Properties.Name.ToString();
                places += "\n";
                place_num = place_num + 1;    
            }
            InventoryLabel = new Label();
            InventoryLabel.Text = places ;
            InventoryLabel.HorizontalOptions = LayoutOptions.CenterAndExpand;
            InventoryLabel.VerticalOptions = LayoutOptions.CenterAndExpand;

            var BackButton = new Button();
            BackButton.HorizontalOptions = LayoutOptions.Start;
            BackButton.VerticalOptions = LayoutOptions.End;
            BackButton.Text = "Back";
            InventoryGrid = new Grid {
                Children = { InventoryLabel, BackButton }
            };
            BackButton.Clicked += BackButton_Clicked;
            this.Content = InventoryGrid;
        }

        private void BackButton_Clicked(object sender, EventArgs e) { this.Content = MapGrid; }

        private void UpdateMoney(object sender, long credit) { MoneyLabel.Text = $"{credit}"; }
    }

    public class CustomMap : Map {
        public Game game;

        public CustomMap(Game game) {
            this.game = game;
        }

    }
}