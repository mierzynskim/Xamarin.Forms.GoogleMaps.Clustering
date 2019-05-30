Xamarin.Forms.GoogleMaps.Clustering Release Notes
----
# 1.0.0
* Finally the library doesn't rely on a custom fork of Xamarin.Forms.GoogleMaps
* ClusterClicked event args contains information about pins and location of a cluster
* Removed Handled property from ClusterClickedEventArgs because it can't be handled on both Android and iOS

# 0.4.0
* Do not allow to update clustered pin until it's rendered. Fixed crash caused by allowing this.
* [Android] Fixed icon cache to support setting icons for clusters containing less markers than minimum bucket size
* [iOS] Changed minimum cluster size to 5 to match Android

# 0.3.2

* Added missing changes from 0.3.1

# 0.3.1

* Correctly set alpha when updating a marker

# 0.3

* BREAKING CHANGE: ClusteredPins collection is removed. You should use Pins collection instead.
* Support for pins collection updates
* Implemented marker dragging

# 0.2

* Added ClusterClicked event
* Prevent marker rendering until custom view is ready on Android
* Removed a hack that disallowed to get info about clicked marker without a label

# 0.1

* Initial beta release. It uses custom fork of Xamarin.Forms.GoogleMaps library until changes are pushed to the main repository. In order to use clustering, please remove Xamarin.Forms.GoogleMaps from your project. 


end of contents
