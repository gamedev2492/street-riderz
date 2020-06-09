
const AWS = require('aws-sdk');
const GameLift = new AWS.GameLift({region: 'us-east-1'});

const StreetRiderzQueueID = 'StreetRiderzQ';
const MaxPlayersPerSession = 200;
const MaxDescribeAttempts = 12; // this will allow 2 minutes for matchmaking with a 10 second sleep

const sleep = delay => new Promise(result => setTimeout(result, delay));

function uuidv4() {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
    var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
    return v.toString(16);
  });
}

exports.handler = async (event) => {
    let response;
    let raisedError = false;
    const latencyMap = event.latencyMap;    // map of AWS regions to latency
    const playerSkill = event.playerSkill;
    
    console.log("startMatchmaking");
    
    await GameLift.startMatchmaking({
            ConfigurationName: "StreetRiderzMatching",
            Players: [{
                "LatencyInMs" : latencyMap,
                "PlayerId" : uuidv4(),
                "PlayerAttributes" : {
                    "skill" : {
                        "N" : playerSkill.toString()
                    }
                }
            }]
        }).
        promise().then(data => {
            response = data.MatchmakingTicket;
        }).
        catch(err => {
            raisedError = true;
            response = err;
        });

    console.log("startMatchmaking complete");

    if(raisedError) {
        return response;
    }
    
    if(response.Status == "FAILED" || response.Status == "CANCELLED" || response.Status == "TIMED_OUT") {
        return response;
    }
    
    let ticketId = [response.TicketId];

    console.log("Got ticket id: " + ticketId.toString());
    
    let attempts = 0;
    let foundSession = false;
    while(!raisedError && !foundSession) {
        attempts++;
        console.log("describeMatchmaking attempt #" + attempts);
        await GameLift.describeMatchmaking({
            TicketIds: [ticketId.toString()]
        }).
        promise().then(data => {
            console.log("Got data: " + JSON.stringify(data));
            response = data.TicketList[0];    // only 1 ticket requested
            console.log(response);
            if(response.Status == "COMPLETED") {
                console.log("** Status COMPLETED **\n");
                foundSession = true;
            } else if(response.Status == "FAILED" || response.Status == "CANCELLED" || response.Status == "TIMED_OUT") {
                console.log("** Status " + response.Status.toString() + " **\n");
                raisedError = true;
            }
        }).
        catch(err => {
            console.log("describeMatchmaking failed:" + err.toString());
            raisedError = true;
            response = err;
        });
        
        if(attempts < MaxDescribeAttempts) {
            await sleep(10000);  // 10 seconds is recommended time to wait to call describeMatchmaking
        } else {
            await GameLift.stopMatchmaking({
                TicketId: ticketId.toString()
            }).
            promise().then(data => {
                response = data; // this will be empty if the stop succeeds   
            }).
            catch(err => {
               response = err; 
            });
            break;
        }
    }
    
    return response;
};
