using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EV
{
    public interface ISelectable
    {
        void OnSelect(PlayerHolder player);
    }

    public interface IDeselect
    {
        void OnDeselect(PlayerHolder player);
    }

    public interface IHighlight
    {
        void OnHighlight(PlayerHolder player);
    }

    public interface IDeHighlight
    {
        void OnDeHighlight(PlayerHolder player, bool endTurnButton);
    }

    public interface IDetectable
    {
        Node OnRaycastHit();
    }

    public interface IHittable
    {
        void OnHit(GridCharacter character);
    }
}
