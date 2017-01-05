/*              SCREAMING INJURED PEDS                     *
 *      a GTA V script made by EnforcerZhukov              *
 * www.enforcerzhukov.xyz | www.youtube.com/enforcerzhukov */
using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;

public class ScreamingInjuredPeds : Script
{
    private List<Ped> injured = new List<Ped>();
    private List<Ped> injuredRmv = new List<Ped>();
    private List<DateTime> execTime = new List<DateTime>();
    private Random r = new Random();
    private DateTime firstDT;
    private int cooldown = 5; //cooldown timer
    private TimeSpan diff;

    public ScreamingInjuredPeds()
    {
        Tick += OnTick;
        Interval = 100;
        firstDT = new DateTime(1999, 1, 1);
    }

    void OnTick(object sender, EventArgs e)
    {
        DateTime dtNow = DateTime.UtcNow;
        foreach (Ped p in World.GetAllPeds()) { //analyze ALL the peds and add them to the Injured list
            if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PED_HURT, p) && p.IsAlive && p.IsHuman && !injured.Contains(p)) {
                injured.Add(p);
                execTime.Add(firstDT); //DateTime element on list with year==1999 -> this ped hasn't been triggered yet
            }
        }

        foreach (Ped p in injured) { //analyze peds in Injured list
            if (p.IsDead) { //if ped is dead, remove it
                injuredRmv.Add(p);
            } else { //if ped is still alive: apply the "injured" screaming sfx
                int pIndex = injured.IndexOf(p);
                bool first = false;
                DateTime dtPrev = execTime[pIndex];
                if (dtPrev.Year == 1999) { //first trigger on this ped
                    first = true;
                } else {
                    diff = dtNow - dtPrev;
                }

                if (first || (!first && diff.Seconds >= cooldown)) { //only execute if first trigger, or cool-down time ok.
                    string faciallib;
                    if (p.Gender == Gender.Female) { //ped is female
                        faciallib = "facials@gen_female@base";
                    } else { //ped is male
                        faciallib = "facials@gen_male@base";
                    }
                   /* PAIN STYLES:
                    * switchr=1 : burning (19)
                    * switchr=2 : coughing (8)
                    * switchr=other values : don't apply
                    */
                    int switchr = r.Next(0, 2);
                    if (switchr == 3) { //coughing pain sfx
                        GTA.Native.Function.Call(GTA.Native.Hash.PLAY_PAIN, p, 19, 1.0f, 0);
                        GTA.Native.Function.Call(GTA.Native.Hash.PLAY_FACIAL_ANIM, p, "coughing_1", faciallib);
                    } else if (switchr == 1 || switchr == 2) { //burning pain sfx
                        GTA.Native.Function.Call(GTA.Native.Hash.PLAY_PAIN, p, 8, 1.0f, 0);
                        GTA.Native.Function.Call(GTA.Native.Hash.PLAY_FACIAL_ANIM, p, "burning_1", faciallib);
                    }
                    if ( (!p.CurrentBlip.Exists()) || (p.CurrentBlip.Sprite != BlipSprite.Dead) ) {
                        p.CurrentBlip.Remove();
                        p.AddBlip();
                        p.CurrentBlip.Sprite = BlipSprite.Dead;
                        p.CurrentBlip.Color = BlipColor.Yellow;
                        p.CurrentBlip.Name = "Laying injured ped";
                    }
                    execTime[pIndex] = dtNow; //Update the datetime on the List with the latest trigger execution.
                } //end of "first"/cooldown time ok.
            }
        }

        foreach (Ped p in injuredRmv) { //process the remove queue
            //int pIndex = injured.IndexOf(p);
            execTime.RemoveAt(injured.IndexOf(p));
            injured.Remove(p);
            GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_PED_PAIN_AUDIO, p, true);
            p.CurrentBlip.Remove();
        }
        injuredRmv.Clear();

    }
}