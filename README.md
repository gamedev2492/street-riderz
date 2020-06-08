# street-riderz
iOS racing game

## Requirements

- An AWS account: <https://aws.amazon.com/getting-started/>
- Unity 2019.3.13f1 or later
- Amazon GameLift Server SDK: <https://aws.amazon.com/gamelift/getting-started/>
- AWS Mobile SDK for Unity: <https://aws.amazon.com/blogs/developer/aws-sdk-for-net-now-targets-net-standard-2-0/>
- Realistic Car Controller  - <https://assetstore.unity.com/packages/tools/physics/realistic-car-controller-16296/>
- CScape City System - <https://assetstore.unity.com/packages/tools/modeling/cscape-city-system-86716/>

## SDK and Assets
- Amazon GameLift: For Multiplayer Matchmaking and Server Hosting

## Building and using the Project

### Step 1: Build the Unity project

1. Open the project in Unity
2. Create a client and server build from Unity. A server build can be created by selecting "Server Build" from the Build Settings dialog.

### Step 2: Upload server build to GameLift

1. Make sure you have the latest AWS command line installed.
2. Upload the server build to GameLift
    - Use us-east-1 region as this is hardcoded
    - Example command:

``` html
aws gamelift upload-build --operating-system [supported OS] --build-root [build path] --name [user-defined name of build] --build-version [user-defined build number] --region [region name]
```

### Step 3: Prepare GameLift

1. Create a new fleet
    - Use us-east-1 region as this is hardcoded
    - Select the build uploaded in step 2.
    - c5.large works well and is in the free tier
    - Fleet type: On-Demand
    - Binary type: Build
    - Set the launch configuration to call "BUILD_NAME" with 1 concurrent process
    - Add port range 7000-8000, protocol UDP, IP address range 0.0.0.0/0
    - Add port range 7000-8000, protocol TCP, IP address range 0.0.0.0/0
    - Don't set a scaling policy on the fleet
2. Create a game session placement queue
    - Use us-east-1 region as this is hardcoded
    - Adding the fleet just created as the only destination.
3. Create matchmaking ruleset using the file AWS/StreetRiderz_MatchmakingRuleSet.json
    - Use us-east-1 region as this is hardcoded
4. Create matchmaking configuration
    - Use us-east-1 region as this is hardcoded
    - Matchmaking configuration must be named "StreetRiderzMatching" so the Lambda can invoke it
    - Use the ruleset and queues you just created
    - Ensure "acceptance required" is set to "no"

### Step 4: Create client service

Refer to the instructions found in step 2 of the article <https://aws.amazon.com/blogs/gametech/creating-servers-for-multiplayer-mobile-games-with-amazon-gamelift/> with the following differences:
    - Call the Lambda ConnectStreetRiderzClient
    - Select node.js Lambda runtime
    - Set the Lambda IAM role using the rules found in AWS/StreetRiderzClient_LambdaIAMRole.json (this differs from step 15-17, you can skip the action editor and just paste in the json)
    - Use the Lambda source code found in AWS/StreetRiderzClient_Lambda.js

### Step 5: Run the game

At this point, you'll be able to run the game client. Note that the way the matchmaking rules are configured, you'll need to connect at least 3 clients before you get a match. You can run these clients on the same machine.

## License Summary

This project is made available under the AGPL-3.0 license. See the LICENSE file.
