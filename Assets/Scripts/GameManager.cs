using UnityEngine;
using UniRx;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using Merusenne.StageGimmick;

/// <summary>
/// �Q�[���S�̂̊Ǘ����s���N���X
/// �V�[���̑J�ځA�Z�[�u�A���[�h�̐�����s��
/// </summary>
public class GameManager : MonoBehaviour
{
    private GameObject _player;                                     //�v���C���[�L�����̏��(Dead��)���擾����̂Ɏg��
    private PlayerCore _playerCore;                                 //�v���C���[���g��Dead�^�O�t���I�u�W�F�N�g�ɐG���Ə�Ԃ����M�����

    private SavePointController[] _savePoints;                      //�V�[����ɂ��鉩�F������
    private SaveDataManager _save;                                  //�Q�[���S�̂ł̃Z�[�u�f�[�^����������(json�`��)

    private GameObject _clearText;                                  //�Q�[���N���A���ɕ\������e�L�X�g�I�u�W�F�N�g
    private ClearTextController _clearTextController;               //_clearText���\�����ꂽ�Ƃ��N���A�t���O�𗧂ĂăV�[���J�ڂ̓��͂��󂯕t����

    [SerializeField] GameObject _tutorial_action;                   //�Q�[�����v���C���ɕ\������`���[�g���A���|�b�v�A�b�v
    
    //�p�����[�^
    [SerializeField] private float _load_wait_time = 2.0f;                  //�v���C���[��Dead���Ă��烍�[�h�����܂ł̎���
    [SerializeField] private float _pop_up_wait_time = 0.5f;                //�Q�[���J�n����`���[�g���A���\���܂ł̎���

    private string _sceneName = "StageScene";                               //���[�h����V�[����
    private string _filePath;                                               //�Z�[�u�f�[�^�̕ۑ���
    private Vector2 _playerStartPos = new Vector2(-5.5f, -4);               //�Z�[�u�f�[�^���Ȃ��Ƃ��̃v���C���[�̊J�n�ʒu
    private Vector2 _playerPosUp = new Vector2(0, 0.5f);                    //�Z�[�u�|�C���g���̃v���C���[�̊J�n�o���ʒu
    private bool _isClear = false;                                          //�N���A�t���O�A�t���O�����ƃ^�C�g���V�[���֑J�ڂ��邽�߂̓��͂��󂯕t����
    private bool _isTutorial = false;                                       //�`���[�g���A�����J���Ă�����
    

    private Subject<Vector2> _saveStage = new Subject<Vector2>();
    public IObservable<Vector2> OnSaveStage => _saveStage;
    void Awake()
    {
        _player = GameObject.FindWithTag("Player");                             //�v���C���[�擾
        _playerCore = _player.GetComponent<PlayerCore>();                       //�v���C���[�̏�Ԏ擾
        _filePath = Application.dataPath + "/.savedata.json";                   //�Z�[�u�f�[�^�̕ۑ���o�^
        _save = new SaveDataManager();                                          //�Z�[�u�f�[�^�̊Ǘ���擾
        _savePoints = FindObjectsOfType<SavePointController>();                 //�V�[����̑S�ăZ�[�u�|�C���g���擾
        _clearText = GameObject.FindWithTag("ClearText");                       //�uGame Clear�v�ƕ\������e�L�X�g�I�u�W�F�N�g
        _clearTextController = _clearText.GetComponent<ClearTextController>();  //�\�����ꂽ�Ƃ��t���O�𑗐M����X�N���v�g�R���|�[�l���g

        _sceneName = "StageScene";                                  //StageScene�̓v���C���[Dead���Ƀ��[�h�����V�[��
       
        Debug.Log($"���[�h����nowStagePos:{_save._nowStagePos}");


        //�v���C���[Dead���Ɉ�莞�Ԃ������ă����[�h�AOnDestroy����Dispose()�����悤�ɓo�^
        _playerCore.OnDead
            .Subscribe(_ => WaitGameRestart())
            .AddTo(this);

        //�S�ẴZ�[�u�|�C���g���w��
        foreach (var savePoint in _savePoints)
        {
            //�Z�[�u�|�C���g�ɐG�ꂽ�Ƃ��Z�[�u�f�[�^���X�V�AOnDestroy����Dispose()�����悤�ɓo�^
            savePoint.OnTriggerSave
                .Subscribe(x =>
                {
                    _save._nowSavePos = x;

                    Debug.Log($"�Z�[�u�|�C���g�̈ʒu��ύX���܂���:{x}");
                    Save();
                })
                .AddTo(this);

            savePoint.OnTriggerStage
                .Subscribe(x =>
                {
                    _save._nowStagePos = x;
                    Debug.Log($"�Z�[�u�|�C���g�̂���X�e�[�W�̈ʒu��ύX���܂���{x}");
                    Save();
                })
                .AddTo(this);
        }

        //�N���A�e�L�X�g���\�����ꂽ�Ƃ��N���A�t���O�𗧂Ă�
        _clearTextController.OnClear
            .Subscribe(_ => _isClear = true)
            .AddTo(this);

        //�Z�[�u�f�[�^�̃��[�h
        Load();
        _saveStage.OnNext(_save._nowStagePos);                              //���[�h���̃X�e�[�W�̑��M�ACameraManager���w��
       
    }

    private void Start()
    {
       
        _player.transform.position = _save._nowSavePos + _playerPosUp;      //���[�h���̃v���C���[�̊J�n�ʒu

        //�Q�[�����v���C�̂Ƃ�����`���[�g���A����\������,���̃|�b�v�A�b�v���\�������̂͏���v���C���̂�
        if (!_save._isFisrtPlay)
        {
            _save._isFisrtPlay = true;                                  //�Z�[�u�f�[�^�ɏ�������
            Observable.Timer(TimeSpan.FromSeconds(_pop_up_wait_time))   //��莞�Ԍo�ߌ�A�`���[�g���A����\������
                .Subscribe(_ =>
                {
                    _isTutorial = true;
                    _tutorial_action.GetComponent<PopUpController>().Open();
                    Debug.Log("�Q�[�����v���C�ɂ��A����`���[�g���A����\�����܂�");
                })
                .AddTo(this);
        }

    }

    private void Update()
    {
        //�N���A�t���O�����������A���͂��s���ƃ^�C�g���V�[���ɖ߂�
        if (_isClear)
        {
            if (Input.anyKeyDown)
            {
                _sceneName = "TitleScene";              //TitleScene�̓Q�[���N�����̍ŏ��̃V�[���A�N���A���ɖ߂��Ă���
                _save._nowSavePos = _playerStartPos;    //�Q�[���J�n�ʒu��������
                Save();                                 //���[�h�O�ɃZ�[�u���s��
                SceneManager.LoadScene(_sceneName);     //�^�C�g���V�[�������[�h
            }
        }

        //����p�̃`���[�g���A���\�����Ɉړ����͂��s���ƈ�莞�Ԍo�ߌ�A�`���[�g���A��������
        if (_isTutorial)
        {
            if (Input.GetKeyDown(KeyCode.W)||Input.GetKeyDown(KeyCode.A)||Input.GetKeyDown(KeyCode.S)||Input.GetKeyDown(KeyCode.D))
            {
                _isTutorial = false;
                Observable.Timer(TimeSpan.FromSeconds(_pop_up_wait_time))
                    .Subscribe(_=>_tutorial_action.GetComponent<PopUpController>().Close());
            }
        }
    }

    //�Z�[�u�f�[�^���X�V���Z�[�u����
    public void Save()
    {
        
        Debug.Log($"�Z�[�u����nowSavePos:{_save._nowSavePos}");
        string json = JsonUtility.ToJson(_save);                    //�Z�[�u�f�[�^��json������ɕϊ�
        StreamWriter streamWriter = new StreamWriter(_filePath);    //_filePath��json�������ۑ�����e�L�X�g�t�@�C���쐬
        streamWriter.Write(json); streamWriter.Flush();             //json������̏������݁A�������ݑ���̊m��
        streamWriter.Close();                                       //�t�@�C������ď������ݏI��
    }

    //�Q�[���J�n���A�v���C���[Dead���Ƀ��[�h����
    public void Load()
    {
        if (File.Exists(_filePath))     //�t�@�C�������݂���Ƃ�
        {
            StreamReader streamReader = new StreamReader(_filePath);    //_filePath��json�������ǂݍ��ރt�@�C���쐬
            string data = streamReader.ReadToEnd();                     //�t�@�C���S�̂�ǂݍ���data�Ɋi�[
            streamReader.Close();                                       //�t�@�C������ēǂݍ��ݏI��
            _save = JsonUtility.FromJson<SaveDataManager>(data);        //json��������Z�[�u�f�[�^�ɕϊ�
            Debug.Log($"���[�h����nowSavePos:{_save._nowSavePos}");
            
        }
        else                           //�t�@�C�������݂��Ȃ��Ƃ�
        {
            Debug.Log("�Z�[�u�f�[�^��������܂���B�V�����Q�[�����J�n���܂��B");
            _save._nowSavePos = _playerStartPos;                        //�f�t�H���g�̃v���C���[�̊J�n�ʒu���i�[
        }

        
    }

    //�v���C���[Dead���Ɉ�莞�ԑ҂�
    void WaitGameRestart()
    {
        Observable.Timer(TimeSpan.FromSeconds(_load_wait_time)).Subscribe(_ => GameRestart());
    }

    //�v���C���[Dead���Ɉ�莞�Ԍo�ߌ�A�Z�[�u�f�[�^���Z�[�u�����ナ���[�h����
    private void GameRestart()
    {
        
        SceneManager.LoadScene(_sceneName);
    }


}