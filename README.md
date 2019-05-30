## ![](logo.png) Xamarin.Forms.GoogleMaps.Clustering 

This library brings support for clustering for library [Xamarin.Forms.GoogleMaps](https://github.com/amay077/Xamarin.Forms.GoogleMaps)

## Usage 

To use the library you only need to add the following line to `AssemlyInfo.cs` or `Main.cs`/`MainActivity.cs` file in your Android and iOS projects. It overrides the default MapRenderer provided in `Xamarin.Forms.GoogleMaps` and add awesome clustering funcionality :)

```csharp
[assembly: ExportRenderer(typeof(ClusteredMap), typeof(ClusteredMapRenderer))]
``` 

## Releases

See [RELEASE_NOTES](RELEASE_NOTES.md).

## Future plans

I'm looking forward to your contribution! Feel free to grab any of available issues and submit a PR. Please ping me [@mierzynskim](https://github.com/mierzynskim) in an issue thread before starting any work. It will help me and others to avoid duplications :)

## Credits
Credits to all involved in development, testing and discussion in the [original issue thread](https://github.com/amay077/Xamarin.Forms.GoogleMaps/issues/123). Special thanks to [@YahavGB](https://github.com/YahavGB) who implemented a big part of clustering solution.

## License

* logo.png by [alecive](http://www.iconarchive.com/show/flatwoken-icons-by-alecive.html) - [CC BY-SA 4.0](https://creativecommons.org/licenses/by-sa/4.0/deed)
* Android Icon made by [Hanan](http://www.flaticon.com/free-icon/android_109464) from www.flaticon.com
* Apple Icon made by [Dave Gandy](http://www.flaticon.com/free-icon/apple-logo_25345) from www.flaticon.com

