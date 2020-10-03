﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class respawn : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;            //holds reference to the respawn point transform

    private void OnCollisionEnter2D(Collision2D collision)
    {
        StartCoroutine(WaitToMove(collision.transform));        //starts coroutine and passes the collided tranform to it
    }

    private IEnumerator WaitToMove(Transform collisionTransform)
    {
        yield return new WaitForSeconds(2f);                    //waits 2 secs
        collisionTransform.position = respawnPoint.position;    //changes collided tranform position
    }
}
