﻿namespace GameObjects.Creatures;

public class Cornotaurus : Creature
{
    public Cornotaurus(int line, Player owner) : base(line, owner, LandType.CornFields, 2, 10, 2)
    {
        Owner.Opponent.TakeDamage(Owner.ControlledLands(LandType));
    }

    public override void ExecuteSkill() { }
}