Mayday, mayday! Ending the Kinecting AR Drone series.
==============

> Disclaimer – This application is not finished and needs additional work

As you can read in my blog post ([link](http://www.kinectingforwindows.com/2014/08/07/mayday-mayday-ending-the-kinecting-ar-drone-series/)) I don't have time left to finish this series but I'll open-source the code anyway.

![K4W logo](http://www.kinectingforwindows.com/wp-content/themes/twentyten/images/headers/logo.jpg)

## Application Status ##
Currently you are able to enter your “battle station” as a “Commander” and take-off by using speech commands while monitoring the Kinect & Drone cameras. You can blink the drone LEDs, perform some tricks and the foundation of flying with your arms.

## Kinect Features ##
This application is based on the following Kinect features

- Camera
- Skeletal Tracking
- Speech recognition
 
## Flying Gestures ##
Here is the list of gestures that are being used to fly the drone -

- <b>Fly up</b> -  Move both your hand above your head <i>(25° Angle)</i></li>
<li><b>Fly down</b> -  Move both your hand below your shoulder your head <i>(25° Angle)</i></li>
<li><b>Move left</b> -  Move left hand below your shoulders and right hand above your head<i>(25° Angle)</i></li>
<li><b>Move right</b> -   Move right hand below your shoulders and left hand above your head<i>(25° Angle)</i></li>
<li><b>Move forward</b> -  Lean forward</li>
<li><b>Move backwards</b> -  Lean backward</li>
<li><b>Rotate left</b> - Rotate your arms counter clock wise with your spine as a center</li>
<li><b>Rotate right</b> - Rotate your arms clock wise with your spine as a center</li>

The gestures are partially developed but not flying smoothly as I want it to be.

## Credits ##
This application uses the [AR.Drone](https://github.com/Ruslan-B/AR.Drone)-library from [Ruslan Balanukhin](https://twitter.com/rbalanukhin).