// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.FantasyModel
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    // nested complex type
    [ComplexType]
    public class CreatureDetails
    {
        public string Name { get; set; }
        public Attributes Attributes { get; set; }
    }

    // multi - level inheritance (TPH)
    public abstract class Creature
    {
        public int Id { get; set; }
        public CreatureDetails Details { get; set; }
    }

    public abstract class Animal : Creature
    {
        public bool TransmitsDesease { get; set; }
    }

    public interface ICarnivore
    {
        ICollection<Animal> Eats { get; set; }
    }

    public class Carnivore : Animal, ICarnivore
    {
        // 1 - Many self reference to base type, interface implementation
        public virtual ICollection<Animal> Eats { get; set; }
    }

    public class Herbivore : Animal
    {
        public string FavoritePlant { get; set; }
    }

    public class Omnivore : Herbivore, ICarnivore
    {
        // 1 - Many self reference to base type, interface implementation
        public virtual ICollection<Animal> Eats { get; set; }
    }

    public class Monster : Creature
    {
    }

    public class Troll : Monster
    {
        // column name "Discriminator" on TPH hierarchy
        public int Discriminator { get; set; }
    }
}
