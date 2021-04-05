using System.Collections;
using System.Collections.Generic;

public enum ProxyEvent {
    Created,
    Deleted,
    StartManipulation,
    EndManipulation,
    StartMoving,
    EndMoving,
    StartScalingAndRotating,
    EndScalingAndRotating,
    HandEntered,
    HandExited,
    HeadEntered,
    HeadExited,
    ObjectEntered,
    ObjectExited,
    AnchoringStart,
    AnchoredProxy,
    AnchoredMark,
    DetachedProxy,
    DetachedMark,
    Minimized,
    Maximized,
    HoverStart,
    HoverEnd
}
