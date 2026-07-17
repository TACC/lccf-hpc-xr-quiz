# LCCF HPC XR - AR SuperCity zSpace Quiz

SuperCity Emergency is an interactive 3D quiz designed as the assessment for the AR SuperCity educational experience. Developed for zSpace hardware using Unity, this quiz tests users' comprehension of supercomputer architecture by challenging them to fix a broken "SuperCity" using the hardware analogies they learned in the AR exhibit.

Users interact with the system using the zSpace stylus, dragging and dropping 3D components in a virtual environment.

## Overview
The user must diagnose six failing city systems (the analogies) and patch them with the correct supercomputer component.

The game operates on a dual-layer progression for each puzzle:

The City Layer (Diagnosis): Users are presented with a failing city metaphor (e.g., an overheating AC unit) and must select the correct hardware label to fix it.

The Placement Layer (Assembly): Users grab the actual 3D hardware component and physically snap it into the correct slot on the central motherboard.

## Hardware & Software Requirements
Because this module relies on 3D and spatial tracking, it cannot be run in a standard web browser or desktop environment.

Hardware: A zSpace enabled laptop or display (e.g., zSpace Inspire) with the zSpace Stylus.

Engine: Unity (Note: zSpace is only compatible with Unity versions 2017.x – 2022.x).
- Required Version: [Unity 2022.3.22f1](https://unity.com/releases/editor/archive)

Plugins: zCore Unity Plugin (for ZCameraRig, ZProvider, and ZPointer functionality).
- [Download zSpace Plugins & SDKs](https://developer.zspace.com/downloads)

## Installation & Setup

### 1. Clone the Repository

```bash
git clone https://github.com/TACC/lccf-hpc-xr-quiz.git
cd lccf-hpc-xr-quiz
```

### 2. Open in Unity
- Open Unity Hub and select Add project from disk.
- Navigate to the cloned `lccf-hpc-xr-quiz` folder and open it.
- Open `SuperCityQuiz.unity` located in the `Assets/` folder.

### 3. Running the App
- Ensure your zSpace tracking services are running (IR lights on the bezel should be active).
- Press Play in the Unity Editor.
- Use the zSpace stylus front button to grab, drag, and drop components.

## Development Team
| Name | Role | Contact |
|-----|-----|-----|
| Andrew Solis | Principal Investigator | [asolis@tacc.utexas.edu](mailto:asolis@tacc.utexas.edu) |
| MJ Johns | Senior UX Researcher | [mjjohnsdesigner.com](https://mjjohnsdesigner.com) |
| Jo Wozniak | RESA IV | [tacc.utexas.edu/about/staff-directory/jo-wozniak](https://tacc.utexas.edu/about/staff-directory/jo-wozniak) |
| Karen Heckel | Software Engineer | [linkedin.com/in/karen-heckel](https://linkedin.com/in/karen-heckel) |
| Sanika Goyal | Experience Design Lead | [sanikagoyal.com](https://sanikagoyal.com) |
| Imelda Ishiekwene | UX Designer | |

(Note: This quiz module is a companion to the main AR SuperCity web experience. For the mobile web-AR repository, see [lccf-xr](https://github.com/TACC/lccf-hpc-xr).)