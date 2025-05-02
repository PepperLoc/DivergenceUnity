using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpecialTileType
{
    Landmine,
    Freeze,
    Paintball,
    Teleporter
}

public class SpecialTileTrigger : MonoBehaviour
{
    public SpecialTileType tileType;
    public string playerTag = "Player";
    public int damage = 0;
    public int freezeTurns = 0;
    public Color paintballColor = Color.magenta;
    public bool isTeleporter = false;
    public int teleporterId;
    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (other.gameObject.tag == playerTag)
        {
            hasTriggered = true;
            switch (tileType)
            {
                case SpecialTileType.Landmine:
                    TurnBasedMovement tbm = FindObjectOfType<TurnBasedMovement>();
                    if (tbm != null)
                    {
                        tbm.DamagePlayer(other.gameObject, damage);
                    }
                    Destroy(gameObject);
                    break;
                case SpecialTileType.Freeze:
                    TurnBasedMovement tbmFreeze = FindObjectOfType<TurnBasedMovement>();
                    if (tbmFreeze != null)
                    {
                        tbmFreeze.FreezeOpponent(other.gameObject, freezeTurns);
                    }
                    Destroy(gameObject);
                    break;
                case SpecialTileType.Paintball:
                    TurnBasedMovement tbmPaintball = FindObjectOfType<TurnBasedMovement>();
                    if (tbmPaintball != null)
                    {
                        tbmPaintball.PaintballOpponent(other.gameObject, paintballColor);
                    }
                    Destroy(gameObject);
                    break;
                case SpecialTileType.Teleporter:
                    TurnBasedMovement tbmTeleport = FindObjectOfType<TurnBasedMovement>();
                    if (tbmTeleport != null)
                    {
                        tbmTeleport.TeleportPlayer(other.gameObject, teleporterId);
                    }
                    break;
            }
        }
    }
}

