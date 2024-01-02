using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chess
{
    public Chess(GameObject go, Grid pos, EFaction faction)
    {
        this.go = go;
        this.pos = pos;
        this.faction = faction;
    }

    public GameObject go { get; }
    public Grid pos { get; }
    public EFaction faction { get; }
}