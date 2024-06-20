using Oculus.Interaction.Surfaces;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class NaveMeshBake : MonoBehaviour
{
    public Unity.AI.Navigation.NavMeshSurface nms;
    public GameObject Jammo;
    public GameObject Environment;
    void Start()
    {
        nms.BuildNavMesh();
    }


    public void buildnav()
    {
        nms.BuildNavMesh();
        Jammo.SetActive(true);
        Environment.SetActive(true);
    }
}
