using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

[VFXBinder("GameObject/Proxy")]
public class ProxyLinkageVfxBinder : VFXBinderBase {

    public ProxyNode proxy;

    [VFXPropertyBinding("UnityEngine.Color")]
    public ExposedProperty colorProperty;

    [VFXPropertyBinding("UnityEngine.Vector3")]
    public ExposedProperty locationProperty;

    [VFXPropertyBinding("System.Single")]
    public ExposedProperty radiusProperty;

    public override bool IsValid(VisualEffect component) {
        return proxy != null
            && component.HasVector4(colorProperty)
            && component.HasVector3(locationProperty)
            && component.HasFloat(radiusProperty);
    }

    public override void UpdateBinding(VisualEffect component) {
        component.SetVector4(colorProperty, proxy.Color);
        component.SetVector3(locationProperty, proxy.transform.position);
        component.SetFloat(radiusProperty, proxy.Radius);
    }
}