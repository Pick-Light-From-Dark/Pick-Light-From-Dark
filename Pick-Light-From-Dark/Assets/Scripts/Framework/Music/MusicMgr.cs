using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 音乐音效管理器
/// </summary>
public class MusicMgr : BaseManager<MusicMgr>
{
    //播放背景音乐的播放器
    private AudioSource bkMusic = null;

    //背景音乐大小
    private float bkMusicValue = 0.5f;
    public float BkMusicValue => bkMusicValue;

    //用于存在正在播放的音效
    private List<AudioSource> soundList = new List<AudioSource>();
    //音效大小
    private float soundValue = 0.5f;
    public float SoundValue => soundValue;
    //音效是否在播放
    private bool soundIsPlay = true;

    private string currentBKName = "";

    private const string BkMusicPrefsKey = "BkMusicVolume";
    private const string SoundPrefsKey = "SoundVolume";

    private MusicMgr()
    {
        bkMusicValue = PlayerPrefs.GetFloat(BkMusicPrefsKey, 0.5f);
        soundValue = PlayerPrefs.GetFloat(SoundPrefsKey, 0.5f);
        MonoMgr.Instance.AddFixedUpdateListener(Update);
    }


    private void Update()
    {
        if (!soundIsPlay)
            return;

        //暂停的不处理 对象池那边 如果没有音效正在播放 就会回收到对象池中
        //为了避免被对象池移除 我们不做处理
        for (int i = soundList.Count - 1; i >= 0; --i)
        {
            if(!soundList[i].isPlaying)
            {
                //音效播放完毕 如果当前没有使用 我们就把音效片段置空
                soundList[i].clip = null;
                PoolMgr.Instance.PushObj(soundList[i].gameObject);
                soundList.RemoveAt(i);
            }
        }
    }


    //播放背景音乐 - 直接引用版本
    public void PlayBKMusic(AudioClip clip)
    {
        //动态构造一个背景音乐播放器对象 保证它过场景时不销毁
        if(bkMusic == null)
        {
            GameObject obj = new GameObject();
            obj.name = "BKMusic";
            GameObject.DontDestroyOnLoad(obj);
            bkMusic = obj.AddComponent<AudioSource>();
        }

        bkMusic.clip = clip;
        bkMusic.loop = true;
        bkMusic.volume = bkMusicValue;
        bkMusic.Play();
    }

    //播放背景音乐 - Resources加载版本
    public void PlayBKMusic(string name)
    {
        
        if (currentBKName == name)
            return;

        currentBKName = name;

        ResMgr.Instance.LoadAsync<AudioClip>("Sound/BkMusic/" + name, (clip) =>
        {
            PlayBKMusic(clip);
        });
    }

    //停止背景音乐
    public void StopBKMusic()
    {
        if (bkMusic == null)
            return;
        bkMusic.Stop();
    }

    //暂停背景音乐
    public void PauseBKMusic()
    {
        if (bkMusic == null)
            return;
        bkMusic.Pause();
    }

    //设置背景音乐大小
    public void ChangeBKMusicValue(float v)
    {
        bkMusicValue = v;
        PlayerPrefs.SetFloat(BkMusicPrefsKey, v);
        if (bkMusic == null)
            return;
        bkMusic.volume = bkMusicValue;
    }

    /// <summary>
    /// 播放音效 - 直接引用版本
    /// </summary>
    public void PlaySound(AudioClip clip, bool isLoop = false, UnityAction<AudioSource> callBack = null)
    {
        //从对象池获取音效源 得到对应组件
        AudioSource source = PoolMgr.Instance.GetObj("Sound/soundObj").GetComponent<AudioSource>();
        //获取到音效源之前 使用的 停止播放
        source.Stop();

        source.clip = clip;
        source.loop = isLoop;
        source.volume = soundValue;
        source.Play();

        if(!soundList.Contains(source))
            soundList.Add(source);

        callBack?.Invoke(source);
    }

    /// <summary>
    /// 播放音效 - Resources加载版本
    /// </summary>
    public void PlaySound(string name, bool isLoop = false, UnityAction<AudioSource> callBack = null)
    {
        ResMgr.Instance.LoadAsync<AudioClip>("Sound/sound/" + name, (clip) =>
        {
            PlaySound(clip, isLoop, callBack);
        });
    }

    /// <summary>
    /// 停止音效
    /// </summary>
    /// <param name="source">音效源组件</param>
    public void StopSound(AudioSource source)
    {
        if(soundList.Contains(source))
        {
            //停止播放
            source.Stop();
            //从列表移除
            soundList.Remove(source);
            //把音效片段置空 释放内存
            source.clip = null;
            //回收到对象池
            PoolMgr.Instance.PushObj(source.gameObject);
        }
    }

    /// <summary>
    /// 改变音效大小
    /// </summary>
    /// <param name="v"></param>
    public void ChangeSoundValue(float v)
    {
        soundValue = v;
        PlayerPrefs.SetFloat(SoundPrefsKey, v);
        for (int i = 0; i < soundList.Count; i++)
        {
            soundList[i].volume = v;
        }
    }

    /// <summary>
    /// 暂停或恢复所有音效
    /// </summary>
    /// <param name="isPlay">是否是继续播放 true为继续 false为暂停</param>
    public void PlayOrPauseSound(bool isPlay)
    {
        if(isPlay)
        {
            soundIsPlay = true;
            for (int i = 0; i < soundList.Count; i++)
                soundList[i].Play();
        }
        else
        {
            soundIsPlay = false;
            for (int i = 0; i < soundList.Count; i++)
                soundList[i].Pause();
        }
    }

    /// <summary>
    /// 音效清理记录，不用时记得回收回去！
    /// 重要！如果你在测试时记得在 OnDestory 时把它回收回去
    /// 空转时会把它回收回去
    /// 空转时会把它回收回去
    /// 空转时会把它回收回去
    /// </summary>
    public void ClearSound()
    {
        for (int i = 0; i < soundList.Count; i++)
        {
            soundList[i].Stop();
            soundList[i].clip = null;
            PoolMgr.Instance.PushObj(soundList[i].gameObject);
        }
        //清空音效列表
        soundList.Clear();
    }
}
