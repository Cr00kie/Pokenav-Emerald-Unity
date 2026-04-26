using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchMenuHelper : MonoBehaviour
{
    public void setCOOLInSearchResult()
    {
        SearchResultMenu.statUsedToFilterSearch = EPokemonStats.COOL;
    }
    public void setTOUGHInSearchResult()
    {
        SearchResultMenu.statUsedToFilterSearch = EPokemonStats.TOUGH;
    }
    public void setBEAUTYInSearchResult()
    {
        SearchResultMenu.statUsedToFilterSearch = EPokemonStats.BEAUTY;
    }
    public void setSMARTInSearchResult()
    {
        SearchResultMenu.statUsedToFilterSearch = EPokemonStats.SMART;
    }
    public void setCUTEInSearchResult()
    {
        SearchResultMenu.statUsedToFilterSearch = EPokemonStats.CUTE;
    }
}
