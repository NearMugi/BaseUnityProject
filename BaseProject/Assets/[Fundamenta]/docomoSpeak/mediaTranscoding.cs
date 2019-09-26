using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_WINMD_SUPPORT
using Windows.Storage;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
#endif

public class mediaTranscoding : MonoBehaviour
{

#if ENABLE_WINMD_SUPPORT
    void hoge()
    {
        var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

        openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
        openPicker.FileTypeFilter.Add(".m4a");
        openPicker.FileTypeFilter.Add(".wav");

        //StorageFile source = await openPicker.PickSingleFileAsync();
        StorageFile source = await storageFolder.GetFileAsync("D:\#WorkSpace\#PersonalDevelop\BaseUnityProject\BaseProject\おめでとう.m4a");

        var savePicker = new Windows.Storage.Pickers.FileSavePicker();

        savePicker.SuggestedStartLocation =
            Windows.Storage.Pickers.PickerLocationId.VideosLibrary;

        savePicker.DefaultFileExtension = ".wav";
        savePicker.SuggestedFileName = "D:\#WorkSpace\#PersonalDevelop\BaseUnityProject\BaseProject\hoge";

        savePicker.FileTypeChoices.Add("WAVE", new string[] { ".wav" });

        StorageFile destination = await savePicker.PickSaveFileAsync();

        MediaEncodingProfile profile =
                MediaEncodingProfile.CreateWav(AudioEncodingQuality.Medium);
        MediaTranscoder transcoder = new MediaTranscoder();
        PrepareTranscodeResult prepareOp = await
            transcoder.PrepareFileTranscodeAsync(source, destination, profile);
        if (prepareOp.CanTranscode)
        {
            var transcodeOp = prepareOp.TranscodeAsync();
            transcodeOp.Progress +=
                new AsyncActionProgressHandler<double>(TranscodeProgress);
            transcodeOp.Completed +=
                new AsyncActionWithProgressCompletedHandler<double>(TranscodeComplete);
        }
        else
        {
            switch (prepareOp.FailureReason)
            {
                case TranscodeFailureReason.CodecNotFound:
                    System.Diagnostics.Debug.WriteLine("Codec not found.");
                    break;
                case TranscodeFailureReason.InvalidProfile:
                    System.Diagnostics.Debug.WriteLine("Invalid profile.");
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("Unknown failure.");
                    break;
            }
        }        
    }
#endif
    // Start is called before the first frame update
    void Start()
    {
#if ENABLE_WINMD_SUPPORT
        Debug.Log("hoge");
        hoge();
#endif
    }

    // Update is called once per frame
    void Update()
    {

    }
}
