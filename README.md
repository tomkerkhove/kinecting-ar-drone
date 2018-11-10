Kinecting AR Drone
===========
Tutorial on how you can fly your **Parrot AR Drone** by using **Kinect for Windows**. 

> Disclaimer – This application is not finished and needs additional work

## Prerequisites
- Parrot AR Drone
- Kinect for Windows
- Basic C# & WPF knowledge
- Kinect for Windows SDK v1.8 ([link](http://go.microsoft.com/fwlink/?LinkID=323588 "link"))
- Microsoft Speech Platform SDK v11 ([link](http://www.microsoft.com/en-us/download/details.aspx?id=27226 "link"))

## What you will learn

### Part I - Basic UI, Kinect camera & voice recognition ([Code](https://github.com/KinectingForWindows/GIK-KinectingARDrone/tree/Part_I)) 
In part I we will integrate our Kinect into the template by **visualizing the camera** and setting up our **speech recognition**.

- Integrate Kinect sensor in our application
- Visualize the Kinect-state
- Visualize the camera
- Add speech recognition

### Mayday, mayday! Ending the Kinecting AR Drone series. ([Code](https://github.com/KinectingForWindows/GIK-KinectingARDrone/tree/Final-Version)) 
> Disclaimer – This application is not finished and needs additional work

As you can read in my blog post ([link](http://www.kinectingforwindows.com/2014/08/07/mayday-mayday-ending-the-kinecting-ar-drone-series/)) I didn't had the time to finish this series but I'll open-source the code anyway.


#### Application Status
Currently you are able to enter your *battle station* as a *Commander* and take-off by using speech commands. In the meanwhile you can monitor the drone & Kinect cameras.

#### Drone Features
These are some of the features we use of the drone.

- Blinking of the LED
- Perform tricks
- Use camera
- Fly around

#### Kinect Features
This application is based on the following Kinect features

- Camera
- Skeletal Tracking
- Speech recognition

#### Flying Gestures
Here is the list of gestures that are being used to fly the drone -

- **Fly up** - Move both your hand above your head (25° Angle) 
- **Fly down** - Move both your hand below your shoulder your head (25° Angle) 
- **Move left** - Move left hand below your shoulders and right hand above your head(25° Angle) 
- **Move right** - Move right hand below your shoulders and left hand above your head(25° Angle) 
- **Move forward** - Lean forward
- **Move backwards** - Lean backward
- **Rotate left** - Rotate your arms counter clock wise with your spine as a center
- **Rotate right** - Rotate your arms clock wise with your spine as a center

The gestures are partially developed but not flying smoothly as I want it to be.

## Template ##
In this tutorial we will use a template to avoid spending too much time on the interface.
The template contains a basic WPF application where we will **visualize** our **Kinect** *(2)* & **drone** *(4)* cameras as well as the **state** of our **devices**. *(1-3)* In *sector 5* we will **log** some things like our recognized commands, etc.

You can download the template in the [Template-branch](https://github.com/KinectingForWindows/GIK-KinectingARDrone/tree/Template "Template-branch").

## Credits ##
This application uses the [AR.Drone](https://github.com/Ruslan-B/AR.Drone)-library from [Ruslan Balanukhin](https://twitter.com/rbalanukhin).
