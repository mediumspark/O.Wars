#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using PlayFab.AdminModels;
using PlayFab;
using System.Linq;
using System;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var t = target as GameManager;

        if (GUILayout.Button("Sync Store"))
        {
            SetNewCatalogue(); 
        }
        if (GUILayout.Button("Update Store"))
        {
            UpdateGlobalStore();
        }

    }

    List<CatalogItem> newCatalog;
    private void UpdateGlobalStore()
    {
        var request = new UpdateCatalogItemsRequest
        {
            Catalog = newCatalog
        };

        PlayFabAdminAPI.SetCatalogItems(request, OnUpdateRequestAccepted, OnError);
    }

    private void SetNewCatalogue()
    {
        newCatalog = new List<CatalogItem>();

        PlayFabAdminAPI.GetCatalogItems(new GetCatalogItemsRequest(), OnRequestAccepted, OnError);
    }

    private void OnRequestAccepted(GetCatalogItemsResult obj)
    {
        newCatalog = UpdatedCatalogue(obj.Catalog); 
    }

    private List<CatalogItem> UpdatedCatalogue(List<CatalogItem> obj)
    {
        List<CatalogItem> CurrentItemList = obj;
        foreach (UnitSO unit in DatabaseAccessor.LoadFullUnitList())
        {
            if (!CurrentItemList.Where(ctx => ctx.ItemId == unit.InvCode).Any())
            {
                CatalogItem item = new CatalogItem();
                item.ItemId = unit.InvCode;
                item.DisplayName = unit.name; 
                item.ItemClass = "UnitSO";
                item.IsTradable = true; 
                CurrentItemList.Add(item);
            }
        }
        foreach (SpellSO unit in DatabaseAccessor.LoadSpellList())
        {
            if (!CurrentItemList.Where(ctx => ctx.ItemId == unit.InvCode).Any())
            {
                CatalogItem item = new CatalogItem();
                item.ItemId = unit.InvCode;
                item.DisplayName = unit.name; 
                item.IsTradable = true; 
                item.ItemClass = "SpellSO";
                CurrentItemList.Add(item);
            }
        }

        return CurrentItemList;
    }


    private void OnError(PlayFabError obj)
    {
        Debug.Log(obj.ErrorMessage);
    }

    private void OnUpdateRequestAccepted(UpdateCatalogItemsResult obj)
    {
        Debug.Log("Request Accepted");
    }
}
#endif