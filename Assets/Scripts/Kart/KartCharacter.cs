using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;

public class KartCharacter : NetworkBehaviour
{
    public event Action<Vector3> OnTransformChanged;

    [Networked]
    public Vector3 TransformIndex { get; set; }
    private ChangeDetector _changeDetector;
    private static void OnTransformChangedCallback(KartCharacter changed)
    {
        Debug.Log("KartCharacter OnTransformChangedCallback"+ changed.TransformIndex);
        changed.OnTransformChanged?.Invoke(changed.TransformIndex);
    }
    private void Awake()
    {
        Debug.Log("KartCharacter Awake>>" + transform.name);
    }

    public override void Spawned()
    {
        base.Spawned();
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        Debug.Log("KartCharacter Spawned>>" + transform.name);
    }

    private void OnDestroy()
    {
        Debug.Log("KartCharacter OnDestroy>>" + transform.name);
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(TransformIndex):
                    OnTransformChangedCallback(this);
                    break;     
            }
        }
    }
}