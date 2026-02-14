# Arduino Robot Virtual Twin

Unity-based virtual twin for a Makeblock Ranger-style differential-drive robot, with interchangeable simulated/physical components and ML-Agents integration. In practice, it runs a real-time software counterpart in Unity where the same control loop can consume simulated sensors or live BLE telemetry, letting you test behavior and learning workflows against a synchronized robot model. A core goal is to enable the physical Ranger to be controlled by artificial intelligence policies developed and validated in the twin. Compared with a digital twin, which is often lifecycle-focused for monitoring and analytics of a deployed asset, this project is focused on interactive simulation, control validation, and training-time experimentation.

## What This Project Does
- Simulates a Ranger robot in Unity with modular sensors/actuators.
- Supports hybrid setups where Unity reads telemetry from a physical robot over BLE.
- Provides an `ArduinoController` behavior loop and an ML-Agents `RangerAgent` for learning/control.
- Targets transfer of AI policies from simulation to the physical Ranger for real-world control.
- Includes PPO and imitation-learning training configs.

## Tech Stack
- Unity `6000.0.64f1` (`ProjectSettings/ProjectVersion.txt`)
- URP + Input System + TextMeshPro
- Unity ML-Agents (local package reference in `Packages/manifest.json`)
- Windows BLE plugin (`Assets/Scripts/Ble/BleWinrtDll.dll`)

## Project Layout
- `Assets/Scenes/Empty Arena.unity`: main enabled scene for simulation/training.
- `Assets/Scripts/ArduinoController.cs`: robot decision loop (obstacle avoidance, scan logic, motor commands).
- `Assets/Scripts/RangerAgent.cs`: ML-Agents policy/heuristic bridge and reward logic.
- `Assets/Scripts/RangerSpawner.cs`: spawns one or more robots with selectable component types.
- `Assets/Scripts/RangerBuilder.cs`: composes robot from prefab modules.
- `Assets/Scripts/Ble/*`: BLE scanning/subscription + telemetry parsing.
- `Assets/Scripts/*/{Simulated*,Physical*}.cs`: interchangeable module implementations.
- `Config/Ranger.yaml`: PPO training config.
- `Config/Ranger-imitation.yaml`: GAIL + behavioral cloning config.

## Architecture Summary
The robot is built from interchangeable module interfaces:
- Drive: `IDifferentialDrive` (`TransformDifferentialDrive` or `PhysicsDifferentialDrive`)
- Motor encoders: `IMeEncoderOnBoard` (simulated or telemetry-backed)
- Servo: `IServo` (simulated or telemetry-backed)
- LiDAR: `ITFminiS` (raycast simulation or telemetry-backed)
- Ultrasonic: `IMeUltrasonicSensor` (raycast simulation or telemetry-backed)

`RangerSpawner` selects module prefabs and creates one or more robot instances at configured spawn positions.

## Setup
### 1) Open in Unity
1. Open this folder in Unity Hub.
2. Use Unity Editor version `6000.0.64f1`.
3. Open scene: `Assets/Scenes/Empty Arena.unity`.

### 2) Verify ML-Agents package path
`Packages/manifest.json` currently references a local path:
- `"com.unity.ml-agents": "file:E:/Users/danig/Workspace/Unity/ml-agents-develop/com.unity.ml-agents"`

If your machine uses a different path, update this entry or replace with your preferred ML-Agents package source.

### 3) Python side (for training)
Install ML-Agents Python tools in your environment (example):
```bash
pip install mlagents
```

## Running the Simulation
1. In `Empty Arena`, configure `RangerSpawner` in Inspector:
- Choose module types (simulated vs physical).
- Set `rangerPosition` list size/positions.
2. Press Play.
3. Observe robot telemetry in assigned `TerminalDisplay` UI components.

## ML-Agents Usage
Behavior name in code/config is `Ranger`.

Example training commands:
```bash
mlagents-learn Config/Ranger.yaml --run-id ranger_ppo
mlagents-learn Config/Ranger-imitation.yaml --run-id ranger_imitation
```
Then press Play in Unity to connect the environment.

Notes:
- `Config/Ranger.yaml` uses PPO + curiosity reward.
- `Config/Ranger-imitation.yaml` uses GAIL + behavioral cloning with demos in `Assets/Resources/Demonstrations`.

## Physical Robot / BLE Mode
`RangerBle` scans and subscribes to a BLE device name containing `Makeblock_LE703e97f555d4` and parses telemetry payloads.

To run telemetry-backed mode:
1. Select physical prefab variants in `RangerSpawner` (servo/lidar/motor/ultrasonic as needed).
2. Ensure BLE hardware + device naming/service UUIDs match your robot firmware.
3. Run on Windows (BLE plugin is WinRT DLL-based).

## Current Limitations
- Physical write-back commands are not implemented yet in several modules:
- `Assets/Scripts/Servo/PhysicalServo.cs` (`Write` TODO)
- `Assets/Scripts/MeEncoderOnBoard/PhysicalMeEncoderOnBoard.cs` (`SetCurrentSpeed`, `StopMotor` TODO)
- `Assets/Scripts/MeUltrasonicSensor/PhysicalMeUltrasonicSensor.cs` has `GetDistanceCm()` not implemented.
- BLE configuration values are currently hardcoded in `RangerBle`.

## Recommended Next Improvements
1. Implement write commands for physical actuator classes.
2. Complete `PhysicalMeUltrasonicSensor.GetDistanceCm()`.
3. Move BLE identifiers and controller constants to external config.
4. Add automated PlayMode tests for sensor adapters and reward logic.




