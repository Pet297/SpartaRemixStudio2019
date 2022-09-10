# Sparta Remix Studio (2019)

Sparta Remix Studio (SRS) is an audio and video editing software, aimed at the creation of a specific type of music and asociated videos, the YTPMV and more specifically, the Sparta Remix.
The difference between SRS and most other video editing software is that SRS is well aware of the idea of reusing video frames. In Sparta Remixes, it is common that the same video clip is played hundreds of times. Instead of reading the same video file over and over, the needed frames are stored in memory making SRS less laggy than most alternatives, thus making it a less general-purpose video editor and more specific-purpose video editor.

## Project status
The SRS project of 2019 has been abandoned due to poor codebase and not-so-good GUI. Only around 10 remixes were created in it (3 by me, the author). The project itself continues with a new and more cleaner codebase. As of now, the newer version isn't usable.

## Usage guide
1) Clone the repository.
2) Download the needed NuGet packages.
3) Build using Visual Studio. (Don't run it yet.)
4) Into the debug or release directory, add the following directories:
  * FFMPEG
  * Projects
  * rendered
  * VideoDescriptors
5) From this website ( https://ffmpeg.org/download.html ), download Windows executables and DLLs for FFMPEG.
   SRS was built for version 4 of FFMPEG and it wasn't tested against version 5.
   Place the unpacked executables into the FFMPEG directory, so the resulting structure is:
   
   .../Debug/FFMPEG/bin/x64/[executables]
   
   possibly replacing "Debug" with "Release" and "x64" with "x86".
6) SRS can now be opened and it should run as expected.
7) To use SRS itself, watch this video and skip to 2:14, since you have built the code yourself insted of downloading the prebuilt version.
   https://www.youtube.com/watch?v=JxYknHFAVaM&list=PLjK-dObMwrGLBk1rDr5xyv2dXWJJsQVN2&index=1
