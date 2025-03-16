using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPowerup : SpawnedPowerup {
    public override void Init(KartEntity spawner) {
        base.Init(spawner);

        Debug.Log("BoostPowerUp Init>>" + spawner.name);
        spawner.Controller.GiveBoost(false, 4);
        
        // Runner.Despawn(Object, true);
        // Destroy(gameObject);
    }

    public override void Spawned() {
        base.Spawned();

        Runner.Despawn(Object);
    }
}
