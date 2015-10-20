MRoundedButton is a subclass of UIControl and the appearance is like the iOS 7 **Phone** app button or the button on the **Control Center** and also perform fade in/out animation for touch event.

Four button styles are suplied:

    RoundedButton.Default           //  central text
    RoundedButton.Subtitle          //  text with subtitle 
    RoundedButton.CentralImage      //  central image
    RoundedButton.ImageWithSubtitle //  image with subtitle

To set a transparent border:
    
    RoundedButton button = new RoundedButton (<BUTTON_FRAME>, <BUTTON_STYLE>);
    button.BorderWidth = 2;
    button.BorderColor = UIColor.Clear;

The value of `BorderWidth` and `CornerRadius` are limited to `Math.Min(<BUTTON_WIDTH> / 2, <BUTTON_HEIGHT> / 2)`. 
Setting the `CornerRadius` to `float.MaximumValue` or `RoundedButton.MaxValue` can easily make a round button.

## RoundedButtonAppearanceManager

RoundedButtonAppearanceManager is the appearance manager for RoundedButton, each appearance information can be stored in a [NSDictionary][1] object to make it reusable in the whole project:

    NSDictionary appearanceProxy1 = NSDictionary.FromObjectsAndKeys (new object[] {
        40,
        2,
        UIColor.Clear,
        UIColor.Black,
        UIColor.White,
        UIColor.White,
        UIColor.Clear 
    }, new string[] {
        RoundedButtonAppearanceKeys.CornerRadius,
        RoundedButtonAppearanceKeys.BorderWidth,
        RoundedButtonAppearanceKeys.BorderColor,
        RoundedButtonAppearanceKeys.ContentColor,
        RoundedButtonAppearanceKeys.ContentAnimateToColor,
        RoundedButtonAppearanceKeys.ForegroundColor,
        RoundedButtonAppearanceKeys.ForegroundAnimateToColor
    });
    RoundedButtonAppearanceManager.RegisterAppearanceProxy(appearanceProxy1, #<UNIQUE_IDENTIFIER>);
    
## HollowBackgroundView

HollowBackgroundView can be used to place the RoundedButton on an image view or any other view, and the superview will be displayed from the hollowed shapes.


[1]:http://iosapi.xamarin.com/?link=T:Foundation.NSDictionary