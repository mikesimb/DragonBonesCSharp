﻿using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DragonBones
{
	public enum SortingMode
    {
		SortByZ,
		SortByOrder
	}

    /**
     * @inheritDoc
     */
	[ExecuteInEditMode,DisallowMultipleComponent]
    public class UnityArmatureComponent : DragonBoneEventDispatcher, IArmatureProxy
    {
        private bool _disposeProxy = true;
        /**
         * @private
         */
        internal Armature _armature = null;
        /**
         * @private
         */
        public UnityDragonBonesData unityData = null;
        /**
         * @private
         */
        public string armatureName = null;
        /**
         * @private
         */
        public string animationName = null;

        internal readonly ColorTransform _colorTransform = new ColorTransform();
        [SerializeField]
        internal string _sortingLayerName = "Default";
        [SerializeField]
        internal int _sortingOrder = 0;

        [Tooltip("0 : Loop")]
        [Range(0, 100)]
        [SerializeField]
        protected int _playTimes = 0;

        [Range(-2f, 2f)]
        [SerializeField]
        protected float _timeScale = 1.0f;
        [SerializeField]
        protected float _zSpace = 0.0f;
        public bool isUGUI = false;
        public bool zorderIsDirty = false;
        
        public SortingMode sortingMode = SortingMode.SortByZ;
        public bool flipX = false;
        public bool flipY = false;
        public bool addNormal = false;
        public GameObject bonesRoot;
        public List<UnityBone> unityBones = null;
        public bool boneHierarchy = false;

        private List<Slot> _sortedSlots = null;
        /**
         * @private
         */
        public void DBClear()
        {
            bonesRoot = null;
            if (_armature != null)
            {
                _armature = null;
                if (_disposeProxy)
                {
                    UnityFactoryHelper.DestroyUnityObject(gameObject);
                }
            }

            _disposeProxy = true;
            _armature = null;
            unityData = null;
            armatureName = null;
            animationName = null;
            _colorTransform.Identity();
            _sortingLayerName = "Default";
            _sortingOrder = 0;
            _playTimes = 0;
            _timeScale = 1.0f;
            _zSpace = 0.0f;
            isUGUI = false;
            zorderIsDirty = false;
            sortingMode = SortingMode.SortByZ;
            flipX = false;
            flipY = false;
            addNormal = false;
            unityBones = null;
            boneHierarchy = false;
            _sortedSlots = null;
        }
        public void DBInit(Armature armature)
        {
            this._armature = armature;
        }

        public void DBUpdate()
        {

        }
        /**
         * @inheritDoc
         */
        public void Dispose(bool disposeProxy = true)
        {
            _disposeProxy = disposeProxy;

            if (_armature != null)
            {
                _armature.Dispose();
            }
        }
        /**
         * @language zh_CN
         * 获取骨架。
         * @readOnly
         * @see DragonBones.Armature
         * @version DragonBones 4.5
         */
        public Armature armature
        {
            get { return _armature; }
        }
        /**
         * @language zh_CN
         * 获取动画控制器。
         * @readOnly
         * @see DragonBones.Animation
         * @version DragonBones 4.5
         */
        public new Animation animation
        {
            get { return _armature != null ? _armature.animation : null; }
        }
        
        
        public string sortingLayerName
        {
            get { return _sortingLayerName; }
            set
            {
                if (_sortingLayerName == value)
                {
                    //return;
                }
                _sortingLayerName = value;
				if(!isUGUI)
                {
				#if UNITY_5_6_OR_NEWER
					if(_sortingGroup)
                    {
						_sortingGroup.sortingLayerName = value;
					#if UNITY_EDITOR
						if(!Application.isPlaying)
                        {
							EditorUtility.SetDirty(_sortingGroup);
						}
					#endif
						return;
					}
				#endif

					foreach (var render in GetComponentsInChildren<Renderer>(true))
					{
						render.sortingLayerName = value;
						#if UNITY_EDITOR
						if(!Application.isPlaying)
                        {
							EditorUtility.SetDirty(render);
						}
						#endif
					}
				}
            }
        }

        
        public int sortingOrder
        {
            get { return _sortingOrder; }
            set
            {
                if (_sortingOrder == value)
                {
                    //return;
                }
                _sortingOrder = value;
				if(!isUGUI)
                {
					#if UNITY_5_6_OR_NEWER
					if(_sortingGroup)
                    {
						_sortingGroup.sortingOrder = value;
						#if UNITY_EDITOR
						if(!Application.isPlaying)
                        {
							EditorUtility.SetDirty(_sortingGroup);
						}
						#endif
						return;
					}
					#endif
					foreach (var render in GetComponentsInChildren<Renderer>(true))
					{
						render.sortingOrder = value;
						#if UNITY_EDITOR
						if(!Application.isPlaying)
                        {
							EditorUtility.SetDirty(render);
						}
						#endif
					}
				}
            }
        }

        
        public float zSpace
        {
            get { return _zSpace; }
            set
            {
                if (value < 0.0f || float.IsNaN(value))
                {
                    value = 0.0f;
                }

                if (_zSpace == value)
                {
                    return;
                }

                _zSpace = value;

                foreach (var slot in _armature.GetSlots())
                {
                    var display = slot.display as GameObject;
                    if (display != null)
                    {
                        display.transform.localPosition = new Vector3(display.transform.localPosition.x, display.transform.localPosition.y, -slot._zOrder * (zSpace + 0.001f));
						if(!isUGUI)
                        {
							UnitySlot us = slot as UnitySlot;
							if(us.meshRenderer != null)
                            {
								if(sortingMode == SortingMode.SortByOrder)
                                {
									us.meshRenderer.sortingOrder = slot._zOrder;
								}
                                else
                                {
									us.meshRenderer.sortingOrder = sortingOrder;
								}

								us.meshRenderer.sortingOrder = sortingOrder;
							}
						}
					}
                }
            }
        }

        public int playTimes
        {
            set
            {
                _playTimes = value;
            }

            get
            {
                return _playTimes;
            }
        }
        
		public float timeScale
        {
			set
            { 
				_timeScale = value;
				if(_armature != null)
                {
					_armature.animation.timeScale = _timeScale;
				}
			}
			get
            {
				if(_armature != null)
                {
					_timeScale = _armature.animation.timeScale;
				}

				return _timeScale;
			}
		}
	
		
		public List<Slot> sortedSlots
        {
			get
            {
				if(_sortedSlots == null)
                {
					_sortedSlots = new List<Slot>(_armature.GetSlots());
				}

				return _sortedSlots;
			}
		}
        /// <summary>
        /// 骨架之间颜色传递
        /// </summary>
        public ColorTransform color
        {
            get { return this._colorTransform; }
            set
            {
                this._colorTransform.CopyFrom(value);
                
                foreach (var slot in this._armature.GetSlots())
                {
                    slot._colorDirty = true;
                }
            }
        }

		#if UNITY_5_6_OR_NEWER
		internal UnityEngine.Rendering.SortingGroup _sortingGroup;
		public UnityEngine.Rendering.SortingGroup sortingGroup
        {
			get { return _sortingGroup; }
		}
		#endif


		#if UNITY_EDITOR
		private bool _isPrefab()
        {
			return PrefabUtility.GetPrefabParent(gameObject) == null 
				&& PrefabUtility.GetPrefabObject(gameObject) != null;
		}
		#endif

		/**
         * @private
         */
        void Awake()
		{
            #if UNITY_EDITOR
            if (_isPrefab())
            {
                return;
            }
            #endif

            #if UNITY_5_6_OR_NEWER
            if (!isUGUI)
            {
				_sortingGroup = GetComponent<UnityEngine.Rendering.SortingGroup>();
				if(_sortingGroup)
                {
					sortingMode = SortingMode.SortByOrder;
					_sortingLayerName = _sortingGroup.sortingLayerName;
					_sortingOrder = _sortingGroup.sortingOrder;
				}
			}
			#endif

			zorderIsDirty = true;
            //if(unityData != null && (unityData.dragonBonesJSON != null || unityData.dragonBonesBinary != null) && unityData.textureAtlas != null)
            if (unityData != null && unityData.dragonBonesJSON != null && unityData.textureAtlas != null)
            {
				var dragonBonesData = UnityFactory.factory.LoadData(unityData, isUGUI);
				if (dragonBonesData != null && !string.IsNullOrEmpty(armatureName))
				{
					UnityFactory.factory.BuildArmatureComponent(armatureName, unityData.dataName, null, null, gameObject , isUGUI);
				}
			}
            
            if (_armature != null)
            {
                sortingLayerName = sortingLayerName;
                sortingOrder = sortingOrder;
				_armature.flipX = flipX;
				_armature.flipY = flipY;
				_armature.animation.timeScale = _timeScale;

                if (zSpace > 0 || sortingMode == SortingMode.SortByOrder)
                {
                    foreach (var slot in _armature.GetSlots())
                    {
                        var display = slot.display as GameObject;
                        if (display != null)
                        {
                            display.transform.localPosition = new Vector3(display.transform.localPosition.x, display.transform.localPosition.y, -slot._zOrder * (zSpace + 0.001f));
                            if (!isUGUI && sortingMode == SortingMode.SortByOrder)
                            {
                                UnitySlot us = slot as UnitySlot;
                                if (us.meshRenderer != null)
                                {
                                    us.meshRenderer.sortingOrder = slot._zOrder;
                                }
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(animationName))
                {
                    _armature.animation.Play(animationName, _playTimes);
                }

				CollectBones();
            }
        }

		void LateUpdate()
        {
            if (_armature == null)
            {
                return;
            }

			flipX = _armature.flipX ;
			flipY = _armature.flipY ;

			#if UNITY_EDITOR
			if(!Application.isPlaying)
            {
				#if UNITY_5_6_OR_NEWER
				if(!isUGUI)
                {
					_sortingGroup = GetComponent<UnityEngine.Rendering.SortingGroup>();
					if(_sortingGroup)
                    {
						sortingMode = SortingMode.SortByOrder;
						_sortingLayerName = _sortingGroup.sortingLayerName;
						_sortingOrder = _sortingGroup.sortingOrder;
					}
				}
				#endif
				foreach (var slot in _armature.GetSlots())
				{
					var display = slot.display as GameObject;
					if (display != null)
					{
                        display.transform.localPosition = new Vector3(display.transform.localPosition.x, display.transform.localPosition.y, -slot._zOrder * (zSpace + 0.001f));
						if(!isUGUI)
                        {
							UnitySlot us = slot as UnitySlot;
							if(us.meshRenderer!=null)
                            {
								us.meshRenderer.sortingLayerName = _sortingLayerName;
								if(sortingMode == SortingMode.SortByOrder)
                                {
									us.meshRenderer.sortingOrder = slot._zOrder;
								}
                                else
                                {
									us.meshRenderer.sortingOrder = _sortingOrder;
								}
							}
						}
					}
				}
			}
#endif

            //child armatrue
            if (armature.parent != null && armature.parent.armature != null)
            {
                UnityArmatureComponent parentArmature = armature.parent.armature.proxy as UnityArmatureComponent;
                if (parentArmature != null)
                {
                    sortingMode = parentArmature.sortingMode;
                    _sortingOrder = parentArmature.sortingOrder;
                    _sortingLayerName = parentArmature.sortingLayerName;
#if UNITY_5_6_OR_NEWER
                    if (parentArmature.sortingGroup)
                    {
                        if (_sortingGroup == null)
                        {
                            _sortingGroup = gameObject.AddComponent<UnityEngine.Rendering.SortingGroup>();
                        }

                        _sortingGroup.sortingLayerName = parentArmature.sortingGroup.sortingLayerName;
                        _sortingGroup.sortingOrder = armature.parent._zOrder;
                        _sortingOrder = _sortingGroup.sortingOrder;
                        _sortingLayerName = _sortingGroup.sortingLayerName;
                    }
                    else if (_sortingGroup)
                    {
                        DestroyImmediate(_sortingGroup);
                    }
#endif
                }
            }
            
            //QQ
            //if (false && zorderIsDirty)
            //{
            //    _sortedSlots = new List<Slot>(_armature.GetSlots());
            //    _sortedSlots.Sort(delegate (Slot x, Slot y)
            //             {
            //                 return x._zOrder - y._zOrder;
            //             });

            //    for (int i = 0; i < _sortedSlots.Count; ++i)
            //    {
            //        Slot slot = _sortedSlots[i];
            //        var display = slot.display as GameObject;
            //        if (display != null)
            //        {
            //            display.transform.SetSiblingIndex(i * 2);
            //            if (!isUGUI && sortingMode == SortingMode.SortByOrder)
            //            {
            //                UnitySlot us = slot as UnitySlot;
            //                if (us.meshRenderer != null)
            //                {
            //                    us.meshRenderer.sortingOrder = i;
            //                }
            //            }
            //        }
            //    }

            //    zorderIsDirty = false;
            //}

            if (unityBones!=null)
            {
				int len = unityBones.Count;
				for(int i=0;i<len;++i)
                {
					UnityBone bone = unityBones[i];
                    if (bone != null)
                    {
                        bone._Update();
                    }
				}
			}
		}

        /**
         * @private
         */
        void OnDestroy()
        {
            if (_armature != null)
            {
                var armature = _armature;
                _armature = null;

                armature.Dispose();

#if UNITY_EDITOR
                UnityFactory.factory._dragonBones.AdvanceTime(0.0f);
#endif
            }

            unityBones = null;
			_sortedSlots = null;
            _disposeProxy = true;
            _armature = null;
        }



        public void CollectBones()
        {
			if(unityBones != null )
			{
				foreach(UnityBone unityBone in unityBones)
                {
					foreach(Bone bone in armature.GetBones())
                    {
						if(unityBone.name.Equals(bone.name))
                        {
							unityBone._bone = bone;
							unityBone._proxy=this;
						}
					}
				}
			}
		}
		public void ShowBones()
        {
			RemoveBones();
			if(bonesRoot==null)
            {
				GameObject go = new GameObject("Bones");
				go.transform.SetParent(transform);
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.identity;
				go.transform.localScale = Vector3.one;
				bonesRoot = go;
				go.hideFlags = HideFlags.NotEditable;
			}

			if(armature!=null)
			{
				unityBones = new List<UnityBone>();
				foreach(Bone bone in armature.GetBones())
                {
					GameObject go = new GameObject(bone.name);
					UnityBone ub = go.AddComponent<UnityBone> ();
					ub._bone = bone;
					ub._proxy = this;
					unityBones.Add(ub);

					go.transform.SetParent(bonesRoot.transform);
				}

				foreach(UnityBone bone in unityBones)
                {
					bone.GetParentGameObject();
				}
                
                foreach (UnityArmatureComponent child in GetComponentsInChildren<UnityArmatureComponent>(true))
                {
                    if (child != this)
                    {
                        child.ShowBones();
                    }
				}
			}
		}
		public void RemoveBones()
        {
            foreach (UnityArmatureComponent child in GetComponentsInChildren<UnityArmatureComponent>(true))
            {
                if (child != this)
                {
                    child.RemoveBones();
                }
			}

			if(unityBones!=null)
            {
				unityBones = null;
			}

			if(bonesRoot)
            {
				DestroyImmediate(bonesRoot);
			}
		}
    }
}