﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/*
 * physical attack buff item class, inherits from Item
 */
public class PDefBuff : Item
{
    public int buffAmount = 5;

    //override item function with what health consumable does to player
    protected override void ItemConsume(Collider other)
    {
        HeroModel stats = other.gameObject.GetComponent<HeroModel>();
        StartCoroutine(tempBuff(stats));
    }

    protected override IEnumerator tempBuff(HeroModel currentStat)
    {
        //get original stat
        int origStat = currentStat.GetDefence();
        //set stat to include buffs
        currentStat.SetDefence(origStat + buffAmount);

        //buff lasts for 30 seconds
        yield return new WaitForSeconds(30);
        //set stat back to original stat
        currentStat.SetDefence(origStat);
        Destroy(gameObject);
    }

    protected override void rotateAnimation()
    {
        transform.RotateAround(transform.position, transform.up, Time.deltaTime * 90f);
    }
}
