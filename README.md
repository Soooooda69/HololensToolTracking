# On-Device HoloLens 2 Tool Tracking under Occlusion

This project contains a sample scene showcasing multi-HoloLens tool tracking framework to adress the line-of-sight issue. It is enabled by the HoloLens 2 IR tracking pipeline and a PC-Devices communication framework based on TCP/IP. 

The framework can seamlessly combine any number of HoloLens and potentially other devices to achieve robust tool tracking under occlusion.

## Environment
* Unity 2021.3.18f1
* Visual Studio 2022
* Windows 11
* Key dependencies: [PoseHub](https://github.com/jmz3/PoseHub)

## How to use
First make sure your `HoloLens2` is configured with research mode enabled. 
To pre-define your tracking targets (marker geometries and 3d models), adjust or add your own geometry files in `Asset->MarkerConfigs->*`. 
In the CommunicationSample scene, adjust the `Pub/Sub port`, Sub IP adress and topics as you need, make sure they are paired with what you defined on your server. 
The useage of pose processing is described in [PoseHub](https://github.com/jmz3/PoseHub) module.

To run this on the HoloLens2, simply build and deploy. Make sure to use Release settings for the final compile in Visual Studio.

After configuring the connection parameters, simply press the connect button to initiate tracking and communication. Pose information and tracking status will then stream to the DebugConsole on your devices. The tracking status is intuitively represented by the color of the floating cubes, red indicating loss of tracking and green indicating successful tracking.

## Demo
We demonstrate a scenario in which the tool and phantom are initially obscured by an obstacle for the second viewer. However, with the additional tracking information provided by the first viewer, the lost tracking information is successfully recovered.

![Alt Text](ar_demo.gif)
## Thanks
we greatly appreciate Mingxu Liu for his implementation of PC-Unity communication framework. Special thanks to the implementation of on-device IR tool tracking from andreaskeller96 https://github.com/andreaskeller96/HoloLens2-IRTracking-Sample.git.


## License and Citation

If you use this project or the library contained within, please cite the following BibTeX entries:

```BibTeX
@misc{keller2023hl2irtracking,
  author =       {Andreas Keller},
  title =        {HoloLens 2 Infrared Retro-Reflector Tracking},
  howpublished = {\url{https://github.com/andreaskeller96/HoloLens2-IRTracking}},
  year =         {2023}
}
```
A. Keller, HoloLens 2 Infrared Retro-Reflector Tracking. https://github.com/andreaskeller96/HoloLens2-IRTracking, 2023. [Online]. Available: https://github.com/andreaskeller96/HoloLens2-IRTracking

```bibtex
@ARTICLE{10021890,
  author={Martin-Gomez, Alejandro and Li, Haowei and Song, Tianyu and Yang, Sheng and Wang, Guangzhi and Ding, Hui and Navab, Nassir and Zhao, Zhe and Armand, Mehran},
  journal={IEEE Transactions on Visualization and Computer Graphics}, 
  title={STTAR: Surgical Tool Tracking using Off-the-Shelf Augmented Reality Head-Mounted Displays}, 
  year={2023},
  volume={},
  number={},
  pages={1-16},
  doi={10.1109/TVCG.2023.3238309}}

```
A. Martin-Gomez et al., “STTAR: Surgical Tool Tracking using Off-the-Shelf Augmented Reality Head-Mounted Displays,” IEEE Transactions on Visualization and Computer Graphics, pp. 1–16, 2023, doi: 10.1109/TVCG.2023.3238309.

## Contributors
* [Jiaming Zhang](https://github.com/jmz3)
* [Hongchao Shu](https://github.com/Soooooda69)