using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Monopoly {
    public partial class LoginPage : ContentPage {
        public LoginPage() {
            /* LABEL 1: TITLE */
            var label = new Label();
            label.VerticalOptions = LayoutOptions.CenterAndExpand;
            label.HorizontalOptions = LayoutOptions.CenterAndExpand;
            var s = new FormattedString();
            s.Spans.Add(new Span { Text = "Welcome to UCLA Monopoly", FontAttributes = FontAttributes.Italic, FontSize = 60 });
            label.FormattedText = s;

            /* INPUT BOX: NAME */
            var nameEntry = new Entry { Placeholder = "Username" };
            nameEntry.VerticalOptions = LayoutOptions.CenterAndExpand;
            nameEntry.HorizontalOptions = LayoutOptions.CenterAndExpand;

            nameEntry.Completed += Entry_Completed;
            this.Content = new StackLayout {
                Children = { label, nameEntry },
                BackgroundColor = Color.Blue
            };
        }

        async void Entry_Completed(object sender, EventArgs e) {
            var text = ((Entry)sender).Text; //cast sender to access the properties of the Entry
            Game game = new Game(text);
#if __ANDROID__
            await Navigation.PushAsync(new Monopoly.Droid.MapPage(game));
#endif
            //await Navigation.PushAsync(new MapPage());
        }
    }
}
