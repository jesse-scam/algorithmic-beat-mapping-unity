# Real-time and Preprocessed Audio Analysis for Onset Detection (Beat Mapping) Using Spectral Flux

## Description

This code sample demonstrates the topics outlined in my article series "Algorithmic Beat Mapping in Unity"
https://medium.com/giant-scam/algorithmic-beat-mapping-in-unity-intro-d4c2c25d2f27

This is a minimal example of the foundation for detecting beats in audio files in Unity.  Audio analysis is available for real-time audio as well as preprocessing an entire audio file up front.
Both analysis methods use the same spectral flux algorithm that is outlined in this article:
http://www.badlogicgames.com/wordpress/?cat=18&paged=3

## Usage

This code sample was tested using Unity 2017.3.0f3.  It should be compatible with Unity 5 as well.

In the Hierarchy, select `SongController` and enable/disable `Real Time Samples` and `Pre Process Samples`.  All combinations of the two are valid.
Load an audio file into the `SongController->AudioSource`.
Press `Play`.
You should hear the audio being played and see the output of the algorithm in the two `Plot` objects on screen.


## License
 
All code samples besides DSPLib.cs and Complex.cs are covered by the MIT license.  See those source files for information about their external licenses.
Example audio file "Chamber of Secrets for 1" by Terror Pigeon is Copyright Terror Pigeon and, while it may be distributed with this project, it may not be further distributed or used without consent.


## Author

All code samples besides DSPLib.cs and Complex.cs were written by Jesse Keogh.  Jesse Keogh is the Lead Developer / Co-Founder of Giant Scam Industries.
https://twitter.com/giantscam
