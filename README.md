## CINENOVEL (Unity Timeline for Naninovel)


![](https://github.com/restush/Cinenovel/blob/resource-branch/Cinenovel-Preview1.0.gif)

### Usage

Play a timeline.
```
@cine LoremIpsum
```
Play a timeline without waiting.
```
@cine LoremIpsum wait:false
```
Play a timeline at start specific time.
```
@cine LoremIpsum startTime:1.5
```
Play a timeline with allowed input that can be triggered while playing timeline.
```
@cine LoremIpsum allowedSampler:Continue,Pause,Rollback
```
Play multiple timeline at same time.
```
@cine LoremIpsum1 wait:false
@cine LoremIpsum2 wait:false
@cine LoremIpsum3 wait:false
@wait 20
```


