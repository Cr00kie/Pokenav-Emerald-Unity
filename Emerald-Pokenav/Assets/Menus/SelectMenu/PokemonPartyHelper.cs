using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonPartyHelper : MonoBehaviour
{
    // Lo usamos en el menu de condition cuando el usuario presiona el boton de Party Pokemon
    public void SetPokemonPartyFromFile()
    {
        PokemonPartyMenu.SetPartyPokemonFromFile("partyPokemon.json");
    }
}
