using System;

[Flags]
public enum ManipulationMode {
    None = 0,
    Inactive = 1,
    Hover = 2,
    Activating = 4,
    Active = 8,
    Translating = 16,
    ScalingAndRotating = 32,
    Anchoring = 64,
    Inside = 128,
    Minimized = 256
}

public delegate void ManipulationModeChangedDelegate(ManipulationMode from, ManipulationMode to);