using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using WickedCrush.Utility;
using Microsoft.Xna.Framework;
using System.Xml.Linq;

namespace WickedCrush.GameStates
{
    public struct EntityInEditor
    {
        public String entName;
        public Point size, gridPos;
        public Texture2D img, iconImg;

        public int dangerRating;

        public bool flipEnabled, attachedToWall;

        public Direction connectionPoint, facing;
    }
    public class Editor
    {
        #region fields
        private SpriteBatch sb;
        ControlsManager _controls;
        Overlord _overlord;
        Matrix scaleMatrix;

        int gridSize = 64;
        int brushSize = 0;
        int maxBrushSize = 3;

        float zoomLevel;
        float maxZoom = 4f;
        float minZoom = 0.6f;

        Direction editorDirection;

        int editorMode; //0 = selection, 1 = geom, 2 = entities, 3 = enemies, 4 = traps

        int entitySelectionIndex, enemySelectionIndex, trapSelectionIndex;

        Vector2 cursorPosition, cameraPosition;
        Point selectedGrid;

        int[,] solidGeom; // 0=blank, 1=geom, 2=filled_by_entity, 3=needed_by_entity?

        int maxDanger, currentDanger;

        List<EntityInEditor> entityList, enemyChoicesList, entityChoicesList, trapChoicesList;

        EntityInEditor tempEntity;
        bool selectedEntity;

        Texture2D crosshair, empty_grid, empty_grid_thick, black_construction_background, sketchy_block,
            screen_border, brush1, brush2, brush3, dpad, right_bumper, flip_button, blapck,
            build_mode, select_mode, enemy_mode, trap_mode, things_tool, mode_selection_bubble;

        SpriteFont editorFont1;

        bool saving;
        String level_name;
        #endregion

        public Editor(Overlord overlord, ControlsManager controls)
        {
            _controls = controls;
            _overlord = overlord;

            this.Initialize();
        }

        public void Initialize()
        {
            sb = new SpriteBatch(_overlord._gd);

            solidGeom = new int[128, 128];
            

            entityList = new List<EntityInEditor>();
            //entityList = new List<EntityInEditor>();
            //trapList = new List<EntityInEditor>();

            entityChoicesList = new List<EntityInEditor>();
            enemyChoicesList = new List<EntityInEditor>();
            trapChoicesList = new List<EntityInEditor>();

            InitializeEnemies();
            InitializeTraps();
            InitializeEntities();

            selectedEntity = false;

            crosshair = _overlord._cm.Load<Texture2D>(@"editor_graphics/crosshair");
            empty_grid = _overlord._cm.Load<Texture2D>(@"editor_graphics/empty_grid");
            empty_grid_thick = _overlord._cm.Load<Texture2D>(@"editor_graphics/empty_grid_thick");
            black_construction_background = _overlord._cm.Load<Texture2D>(@"editor_graphics/dark_background");
            sketchy_block = _overlord._cm.Load<Texture2D>(@"editor_graphics/sketchy_block");
            screen_border = _overlord._cm.Load<Texture2D>(@"editor_graphics/white_screen_border");
            brush1 = _overlord._cm.Load<Texture2D>(@"editor_graphics/brush1");
            brush2 = _overlord._cm.Load<Texture2D>(@"editor_graphics/brush2");
            brush3 = _overlord._cm.Load<Texture2D>(@"editor_graphics/brush3");
            dpad = _overlord._cm.Load<Texture2D>(@"editor_graphics/dpad");
            right_bumper = _overlord._cm.Load<Texture2D>(@"editor_graphics/right_bumper");
            flip_button = _overlord._cm.Load<Texture2D>(@"editor_graphics/flip_button");

            select_mode = _overlord._cm.Load<Texture2D>(@"editor_graphics/select_mode");
            build_mode = _overlord._cm.Load<Texture2D>(@"editor_graphics/build_mode");
            enemy_mode = _overlord._cm.Load<Texture2D>(@"editor_graphics/enemy_mode");
            trap_mode = _overlord._cm.Load<Texture2D>(@"editor_graphics/trap_mode");
            things_tool = _overlord._cm.Load<Texture2D>(@"editor_graphics/things_tool");
            mode_selection_bubble = _overlord._cm.Load<Texture2D>(@"editor_graphics/mode_selection_bubble");

            blapck = _overlord._cm.Load<Texture2D>(@"editor_graphics/blapck");

            editorFont1 = _overlord._cm.Load<SpriteFont>(@"fonts/EditorFont1");

            SetToDefaults();
            FilledMap();
        }

        private void SetToDefaults()
        {
            maxDanger = 100;
            currentDanger = 0;

            entitySelectionIndex = 0;
            enemySelectionIndex = 0;
            trapSelectionIndex = 0;

            editorMode = 0;
            brushSize = 0;

            saving = false;
            level_name = "";

            editorDirection = Direction.Left;

            cursorPosition = new Vector2(960f, 540f);
            cameraPosition = new Vector2(3136f, -3612f);
            zoomLevel = 1.5f;
        }

        public void Update(GameTime gameTime)
        {
            if (!saving)
            {
                UpdateCursorPosition(gameTime);
                UpdateCameraPosition(gameTime);
                UpdateZoom(gameTime);

                selectedGrid = GetSelectedGrid();

                UpdateMode();

                switch (editorMode)
                {
                    case 0: //select mode
                        UpdateSelectMode();
                        break;
                    case 1: //geom mode
                        UpdateGeomMode();
                        break;
                    case 2: //entity mode
                        //UpdateGeomMode();
                        UpdateEntityMode();
                        break;
                    case 3: //enemy mode
                        UpdateEnemyMode();
                        break;
                    case 4: //traps mode
                        UpdateTrapMode();
                        //UpdateGeomMode();
                        break;
                }

                if (_controls.StartPressed())
                {
                    saving = true;
                    level_name = "";
                }
            }
            else
            {
                if (_controls.GetPressedKey().Length > 0)
                {
                    if ((int)_controls.GetPressedKey()[0] >= 65 && (int)_controls.GetPressedKey()[0] <= 90)
                        level_name += (char)(int)_controls.GetPressedKey()[0];

                    if ((char)(int)_controls.GetPressedKey()[0] == '\r')
                    {
                        SaveLevel(level_name);
                        saving = false;
                    }
                }

                if (_controls.StartPressed())
                {
                    SaveLevel(level_name);
                    saving = false;
                }
            }
        }

        public void Draw(GameTime gameTime)
        {
            _overlord._gd.Clear(Color.Black);

            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, null, scaleMatrix);
            sb.Draw(black_construction_background, new Rectangle(0, 0, 1920, 1080), Color.White);
            DrawGrids();
            DrawEnemyList();

            DrawCrosshair();

            switch (editorMode)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    DrawEntityMode();
                    break;
                case 3:
                    DrawEnemyMode();
                    break;
                case 4:
                    DrawTrapMode();
                    break;
            }

            DrawHUD();

            if (saving)
            {
                sb.Draw(blapck, new Rectangle(10, 950, 1900, 100), Color.Black);
                sb.DrawString(editorFont1, level_name, new Vector2(10f, 1000f), Color.White);
            }
            
            sb.Draw(screen_border, new Rectangle(0, 0, 1920, 1080), Color.White);
            sb.End();
        }

        private void DrawEnemyList()
        {
            SpriteEffects mirror;

            foreach(EntityInEditor e in entityList)
            {
                if (e.facing.Equals(Direction.Left))
                    mirror = SpriteEffects.None;
                else
                    mirror = SpriteEffects.FlipHorizontally;

                sb.Draw(e.img,
                    new Rectangle((int)(e.gridPos.X * 48 * zoomLevel) - (int)cameraPosition.X,
                    1080 - (int)(48 * zoomLevel) - (int)(e.gridPos.Y * 48 * zoomLevel) - (int)cameraPosition.Y,
                    (int)(e.size.Y * 48 * zoomLevel),
                    (int)(e.size.X * 48 * zoomLevel)),
                    null,
                    Color.White,
                    0f,
                    Vector2.Zero,
                    mirror,
                    0f);
            }
        }

        private void DrawHUD()
        {
            sb.Draw(mode_selection_bubble,
                new Rectangle(1400 + 100 * editorMode, 10, 96, 192),
                Color.White);

            sb.Draw(right_bumper,
                new Rectangle(1400 + 100 * editorMode, 202, 96, 48),
                Color.White);

            sb.Draw(select_mode,
                new Rectangle(1400, 10, 96, 192),
                Color.White);
            sb.Draw(build_mode,
                new Rectangle(1400 + 100, 10, 96, 192),
                Color.White);
            sb.Draw(things_tool,
                new Rectangle(1400 + 200, 10, 96, 192),
                Color.White);
            sb.Draw(enemy_mode,
                new Rectangle(1400 + 300, 10, 96, 192),
                Color.White);
            sb.Draw(trap_mode,
                new Rectangle(1400 + 400, 10, 96, 192),
                Color.White);

            DrawDangerRating();
            DrawButtons();
        }

        private void DrawDangerRating()
        {
            sb.DrawString(editorFont1, currentDanger.ToString() + " / " + maxDanger.ToString(),
                new Vector2(1200, 10), Color.White);
        }

        private void DrawButtons()
        {
            switch (editorMode)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    if (tempEntity.flipEnabled == true)
                        sb.Draw(flip_button,
                            new Rectangle(1762, 1000, 128, 64),
                            Color.White);
                    break;
                case 4:
                    if (tempEntity.flipEnabled == true)
                        sb.Draw(flip_button,
                            new Rectangle(1762, 1000, 128, 64),
                            Color.White);
                    break;
            }
        }

        private void DrawEnemyMode()
        {
            SpriteEffects mirror;
            Color placementColor;

            if (editorDirection.Equals(Direction.Left))
                mirror = SpriteEffects.None;
            else
                mirror = SpriteEffects.FlipHorizontally;

            if (CanEntityBePlaced())
                placementColor = Color.White * .75f;
            else
                placementColor = Color.Red * .75f;

            /*sb.Draw(tempEntity.img,
                new Rectangle((int)(tempEntity.gridPos.X * 48 * zoomLevel) - (int)cameraPosition.X,
                1080 - (int)(48 * zoomLevel) - (int)(tempEntity.gridPos.Y * 48 * zoomLevel) - (int)cameraPosition.Y,
                (int)(tempEntity.size.Y*48*zoomLevel),
                (int)(tempEntity.size.X * 48 * zoomLevel)),
                placementColor);*/

            sb.Draw(tempEntity.img, new Rectangle((int)(tempEntity.gridPos.X * 48 * zoomLevel) - (int)cameraPosition.X,
                1080 - (int)(48 * zoomLevel) - (int)(tempEntity.gridPos.Y * 48 * zoomLevel) - (int)cameraPosition.Y, (int)(tempEntity.size.Y * 48 * zoomLevel),
                (int)(tempEntity.size.X * 48 * zoomLevel)),
                null,
                placementColor,
                0f,
                Vector2.Zero,
                mirror,
                0f);
            


            for (int i = 0; i < enemyChoicesList.Count; i++)
            {
                sb.Draw(enemyChoicesList[i].iconImg,
                    new Rectangle(1742, 256 + 158 * i, 128, 128),
                    Color.White);

                sb.DrawString(editorFont1, enemyChoicesList[i].dangerRating.ToString(), new Vector2(1852, 338 + 158 * i), Color.White);
                sb.DrawString(editorFont1, enemyChoicesList[i].dangerRating.ToString(), new Vector2(1850, 336 + 158 * i), Color.Black);
            }

            
            sb.Draw(dpad, new Rectangle(1624, 266 + 158 * enemySelectionIndex, 100, 100), Color.White);
        }

        private void DrawTrapMode()
        {
            SpriteEffects mirror;
            Color placementColor;

            if (editorDirection.Equals(Direction.Left))
                mirror = SpriteEffects.None;
            else
                mirror = SpriteEffects.FlipHorizontally;

            if (CanEntityBePlaced())
                placementColor = Color.White * .75f;
            else
                placementColor = Color.Red * .75f;

            sb.Draw(tempEntity.img, new Rectangle((int)(tempEntity.gridPos.X * 48 * zoomLevel) - (int)cameraPosition.X,
                1080 - (int)(48 * zoomLevel) - (int)(tempEntity.gridPos.Y * 48 * zoomLevel) - (int)cameraPosition.Y, (int)(tempEntity.size.Y * 48 * zoomLevel),
                (int)(tempEntity.size.X * 48 * zoomLevel)),
                null,
                placementColor,
                0f,
                Vector2.Zero,
                mirror,
                0f);

            for (int i = 0; i < trapChoicesList.Count; i++)
            {
                sb.Draw(trapChoicesList[i].iconImg,
                    new Rectangle(1742, 256 + 158 * i, 128, 128),
                    Color.White);

                sb.DrawString(editorFont1, trapChoicesList[i].dangerRating.ToString(), new Vector2(1852, 338 + 158 * i), Color.White);
                sb.DrawString(editorFont1, trapChoicesList[i].dangerRating.ToString(), new Vector2(1850, 336 + 158 * i), Color.Black);
            }

            
            sb.Draw(dpad, new Rectangle(1624, 266 + 158 * trapSelectionIndex, 100, 100), Color.White);
        }

        private void DrawEntityMode()
        {
            SpriteEffects mirror;
            Color placementColor;

            if (editorDirection.Equals(Direction.Left))
                mirror = SpriteEffects.None;
            else
                mirror = SpriteEffects.FlipHorizontally;

            if (CanEntityBePlaced())
                placementColor = Color.White * .75f;
            else
                placementColor = Color.Red * .75f;

            sb.Draw(tempEntity.img, new Rectangle((int)(tempEntity.gridPos.X * 48 * zoomLevel) - (int)cameraPosition.X,
                1080 - (int)(48 * zoomLevel) - (int)(tempEntity.gridPos.Y * 48 * zoomLevel) - (int)cameraPosition.Y, (int)(tempEntity.size.Y * 48 * zoomLevel),
                (int)(tempEntity.size.X * 48 * zoomLevel)),
                null,
                placementColor,
                0f,
                Vector2.Zero,
                mirror,
                0f);

            for (int i = 0; i < entityChoicesList.Count; i++)
            {
                sb.Draw(entityChoicesList[i].iconImg,
                    new Rectangle(1742, 256 + 158 * i, 128, 128),
                    Color.White);

                sb.DrawString(editorFont1, entityChoicesList[i].dangerRating.ToString(), new Vector2(1852, 338 + 158 * i), Color.White);
                sb.DrawString(editorFont1, entityChoicesList[i].dangerRating.ToString(), new Vector2(1850, 336 + 158 * i), Color.Black);
            }


            sb.Draw(dpad, new Rectangle(1624, 266 + 158 * entitySelectionIndex, 100, 100), Color.White);
        }

        private void UpdateGeomMode()
        {
            UpdateBrush();

            if (_controls.EditorErasePressed())
                UseBrushRemove();

            if (_controls.EditorDrawPressed())
                UseBrushAdd();
        }

        private void UpdateEnemyMode()
        {
            if (_controls.DPadDown())
                enemySelectionIndex++;
            if (_controls.DPadUp())
                enemySelectionIndex--;

            if (enemySelectionIndex < 0)
                enemySelectionIndex = enemyChoicesList.Count-1;
            if (enemySelectionIndex >= enemyChoicesList.Count)
                enemySelectionIndex = 0;

            tempEntity = enemyChoicesList[enemySelectionIndex];
            tempEntity.gridPos = selectedGrid;

            if (_controls.ConfirmPressed())
            {
                PlaceEntity();
            }

            if (tempEntity.flipEnabled == false)
            {
                editorDirection = Direction.Left;
            }

            if (_controls.FlipPressed())
            {
                FlipEntity();
            }
        }

        private void UpdateTrapMode()
        {
            if (_controls.DPadDown())
                trapSelectionIndex++;
            if (_controls.DPadUp())
                trapSelectionIndex--;

            if (trapSelectionIndex < 0)
                trapSelectionIndex = trapChoicesList.Count - 1;
            if (trapSelectionIndex >= trapChoicesList.Count)
                trapSelectionIndex = 0;

            tempEntity = trapChoicesList[trapSelectionIndex];
            tempEntity.gridPos = selectedGrid;

            if (_controls.ConfirmPressed())
            {
                PlaceEntity();
            }

            if (tempEntity.flipEnabled == false)
            {
                editorDirection = Direction.Left;
            }

            if (_controls.FlipPressed())
            {
                FlipEntity();
            }
        }

        private void UpdateEntityMode()
        {
            if (_controls.DPadDown())
                entitySelectionIndex++;
            if (_controls.DPadUp())
                entitySelectionIndex--;

            if (entitySelectionIndex < 0)
                entitySelectionIndex = entityChoicesList.Count - 1;
            if (entitySelectionIndex >= entityChoicesList.Count)
                entitySelectionIndex = 0;

            tempEntity = entityChoicesList[entitySelectionIndex];
            tempEntity.gridPos = selectedGrid;

            if (_controls.ConfirmPressed())
            {
                PlaceEntity();
            }

            if (tempEntity.flipEnabled == false)
            {
                editorDirection = Direction.Left;
            }

            if (_controls.FlipPressed())
            {
                FlipEntity();
            }
        }

        private void UpdateSelectMode()
        {
            SelectedEntity();

            if (_controls.CancelPressed())
                RemoveEntity();
        }

        private void FlipEntity()
        {
            if (tempEntity.flipEnabled == true)
            {
                if (editorDirection.Equals(Direction.Left))
                    editorDirection = Direction.Right;
                else
                    editorDirection = Direction.Left;
            }
        }

        private bool CanEntityBePlaced()
        {
            for (int i = 0; i < tempEntity.size.Y; i++)
            {
                for (int j = 0; j < tempEntity.size.X; j++)
                {
                    if (solidGeom[tempEntity.gridPos.Y - j, tempEntity.gridPos.X + i] != 0)
                        return false;
                }
                if (tempEntity.connectionPoint.Equals(Direction.Down))
                    if (solidGeom[tempEntity.gridPos.Y - tempEntity.size.X, tempEntity.gridPos.X + i] != 1
                        && solidGeom[tempEntity.gridPos.Y - tempEntity.size.X, tempEntity.gridPos.X + i] != 3)
                        return false;
            }

            return true;
        }

        private void PlaceEntity()
        {
            if (CanEntityBePlaced())
            {
                tempEntity.facing = editorDirection;
                entityList.Add(tempEntity);
            

                for (int i = 0; i < tempEntity.size.Y; i++)
                {
                    for (int j = 0; j < tempEntity.size.X; j++)
                        solidGeom[tempEntity.gridPos.Y - j, tempEntity.gridPos.X + i] = 2;
                    if(tempEntity.connectionPoint.Equals(Direction.Down))
                        solidGeom[tempEntity.gridPos.Y - tempEntity.size.X, tempEntity.gridPos.X + i] = 3;
                }


                NewEntity();
            
            

                UpdateDangerLevel();

            }
        }

        private void NewEntity()
        {
            
            switch (editorMode)
            {
                case 0: //select mode
                    //UpdateGeomMode();
                    break;
                case 1: //geom mode
                    //UpdateGeomMode();
                    break;
                case 2: //entity mode
                    tempEntity = new EntityInEditor();
                    tempEntity = entityChoicesList[entitySelectionIndex];
                    //UpdateGeomMode();
                    break;
                case 3: //enemy mode
                    tempEntity = new EntityInEditor();
                    tempEntity = enemyChoicesList[enemySelectionIndex];
                    //UpdateEnemyMode();
                    break;
                case 4: //traps mode
                    tempEntity = new EntityInEditor();
                    tempEntity = trapChoicesList[trapSelectionIndex];
                    //UpdateTrapMode();
                    //UpdateGeomMode();
                    break;
            }
            tempEntity.gridPos = selectedGrid;
        }

        private void UpdateDangerLevel()
        {
            int temp = 0;
            foreach (EntityInEditor e in entityList)
                temp += e.dangerRating;

            currentDanger = temp;
        }

        private void UpdateMode()
        {
            if (_controls.RightBumper())
            {
                editorMode++;
                if (editorMode > 4)
                    editorMode = 0;
            }
        }

        private void InitializeEnemies()
        {
            EntityInEditor tempEntity;
            enemyChoicesList = new List<EntityInEditor>();

            tempEntity = new EntityInEditor();
            tempEntity.entName = "TreeMob";
            tempEntity.size = new Point(2, 1); //row, col
            tempEntity.gridPos = new Point(0, 0);
            tempEntity.dangerRating = 5;
            tempEntity.flipEnabled = true;
            tempEntity.attachedToWall = false;
            tempEntity.connectionPoint = Direction.Down;
            tempEntity.facing = Direction.Left;
            tempEntity.img = _overlord._cm.Load<Texture2D>(@"editor_graphics/tree_blueprint");
            tempEntity.iconImg = _overlord._cm.Load<Texture2D>(@"editor_graphics/tree_icon");

            enemyChoicesList.Add(tempEntity);

            tempEntity = new EntityInEditor();
            tempEntity.entName = "Rhino";
            tempEntity.size = new Point(3, 2); //row, col
            tempEntity.gridPos = new Point(0, 0);
            tempEntity.dangerRating = 10;
            tempEntity.flipEnabled = true;
            tempEntity.attachedToWall = false;
            tempEntity.connectionPoint = Direction.Down;
            tempEntity.facing = Direction.Left;
            tempEntity.img = _overlord._cm.Load<Texture2D>(@"editor_graphics/rhino_blueprint");
            tempEntity.iconImg = _overlord._cm.Load<Texture2D>(@"editor_graphics/rhino_icon");

            enemyChoicesList.Add(tempEntity);

            tempEntity = new EntityInEditor();
            tempEntity.entName = "Birdy";
            tempEntity.size = new Point(1, 1); //row, col
            tempEntity.gridPos = new Point(0, 0);
            tempEntity.dangerRating = 5;
            tempEntity.flipEnabled = true;
            tempEntity.attachedToWall = false;
            tempEntity.connectionPoint = Direction.None;
            tempEntity.facing = Direction.Left;
            tempEntity.img = _overlord._cm.Load<Texture2D>(@"editor_graphics/bird_blueprint");
            tempEntity.iconImg = _overlord._cm.Load<Texture2D>(@"editor_graphics/bird_icon");

            enemyChoicesList.Add(tempEntity);
        }

        private void InitializeTraps()
        {
            EntityInEditor tempEntity;
            trapChoicesList = new List<EntityInEditor>();

            tempEntity = new EntityInEditor();
            tempEntity.entName = "Spike_Trap";
            tempEntity.size = new Point(1, 1); //row, col
            tempEntity.gridPos = new Point(0, 0);
            tempEntity.dangerRating = 5;
            tempEntity.flipEnabled = false;
            tempEntity.attachedToWall = false;
            tempEntity.connectionPoint = Direction.Down;
            tempEntity.facing = Direction.Left;
            tempEntity.img = _overlord._cm.Load<Texture2D>(@"editor_graphics/spikes_blueprint");
            tempEntity.iconImg = _overlord._cm.Load<Texture2D>(@"editor_graphics/spike_icon");

            trapChoicesList.Add(tempEntity);

            tempEntity = new EntityInEditor();
            tempEntity.entName = "Flametosser";
            tempEntity.size = new Point(1, 1); //row, col
            tempEntity.gridPos = new Point(0, 0);
            tempEntity.dangerRating = 15;
            tempEntity.flipEnabled = true;
            tempEntity.attachedToWall = false;
            tempEntity.connectionPoint = Direction.Down;
            tempEntity.facing = Direction.Left;
            tempEntity.img = _overlord._cm.Load<Texture2D>(@"editor_graphics/flametosser_blueprint");
            tempEntity.iconImg = _overlord._cm.Load<Texture2D>(@"editor_graphics/flametosser_icon");

            trapChoicesList.Add(tempEntity);

        }

        private void InitializeEntities()
        {
            EntityInEditor tempEntity;
            entityChoicesList = new List<EntityInEditor>();

            tempEntity = new EntityInEditor();
            tempEntity.entName = "Level_Entrance";
            tempEntity.size = new Point(2, 2); //row, col
            tempEntity.gridPos = new Point(0, 0);
            tempEntity.dangerRating = 0;
            tempEntity.flipEnabled = false;
            tempEntity.attachedToWall = true;
            tempEntity.connectionPoint = Direction.Down;
            tempEntity.facing = Direction.Left;
            tempEntity.img = _overlord._cm.Load<Texture2D>(@"editor_graphics/start_blueprint");
            tempEntity.iconImg = _overlord._cm.Load<Texture2D>(@"editor_graphics/start_icon");

            entityChoicesList.Add(tempEntity);

            tempEntity = new EntityInEditor();
            tempEntity.entName = "Level_Exit";
            tempEntity.size = new Point(2, 2); //row, col
            tempEntity.gridPos = new Point(0, 0);
            tempEntity.dangerRating = 0;
            tempEntity.flipEnabled = false;
            tempEntity.attachedToWall = true;
            tempEntity.connectionPoint = Direction.Down;
            tempEntity.facing = Direction.Left;
            tempEntity.img = _overlord._cm.Load<Texture2D>(@"editor_graphics/end_blueprint");
            tempEntity.iconImg = _overlord._cm.Load<Texture2D>(@"editor_graphics/end_icon");

            entityChoicesList.Add(tempEntity);

        }

        private void UpdateCursorPosition(GameTime gameTime)
        {
            cursorPosition += new Vector2(_controls.XAxis() * (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.7f, -_controls.YAxis() * (float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.7f);
            if (cursorPosition.X < 0f)
                cursorPosition.X = 0f;
            if (cursorPosition.Y < 0f)
                cursorPosition.Y = 0f;
            if (cursorPosition.X > 1920f)
                cursorPosition.X = 1920f;
            if (cursorPosition.Y > 1080f)
                cursorPosition.Y = 1080f;
        }

        private void UpdateCameraPosition(GameTime gameTime)
        {
            cameraPosition += new Vector2(_controls.RStickXAxis() * (float)gameTime.ElapsedGameTime.TotalMilliseconds, -_controls.RStickYAxis() * (float)gameTime.ElapsedGameTime.TotalMilliseconds);
            if (cameraPosition.X < 0f)
                cameraPosition.X = 0f;
            if (cameraPosition.Y > 0f)
                cameraPosition.Y = 0f;
            if (cameraPosition.X > solidGeom.GetLength(1) * 48 * zoomLevel - 1920)
                cameraPosition.X = solidGeom.GetLength(1) * 48 * zoomLevel - 1920;
            if (cameraPosition.Y < -solidGeom.GetLength(0) * 48 * zoomLevel + 1080)
                cameraPosition.Y = -solidGeom.GetLength(0) * 48 * zoomLevel + 1080;
        }

        private void UpdateZoom(GameTime gameTime)
        {
            float zoomChangeAmt = (_controls.RightTrigger() * (float)gameTime.ElapsedGameTime.TotalSeconds) - (_controls.LeftTrigger() * (float)gameTime.ElapsedGameTime.TotalSeconds);
            zoomLevel += zoomChangeAmt;

            if (zoomLevel < minZoom)
                zoomLevel = minZoom;
            else if (zoomLevel > maxZoom)
                zoomLevel = maxZoom;
            else
                cameraPosition += new Vector2(zoomChangeAmt * (cameraPosition.X+960+24) / zoomLevel,
                    zoomChangeAmt * (cameraPosition.Y-540-24) / zoomLevel);

            //if (zoomLevel != maxZoom && zoomLevel != minZoom)
                //cameraPosition += new Vector2(zoomChangeAmt * 990f, -zoomChangeAmt * 990f);

        }

        private void UpdateBrush()
        {
            if (_controls.LeftBumper())
            {
                brushSize++;
                if (brushSize > maxBrushSize)
                    brushSize = 0;
            }
        }

        private void DrawGrids()
        {
            for (int row = 0; row < solidGeom.GetLength(0); row++)
            {
                for (int col = 0; col < solidGeom.GetLength(1); col++)
                {
                    if ((col * 48 * zoomLevel - cameraPosition.X < 1920f)
                        && (col * 48 * zoomLevel - cameraPosition.X > -48 * zoomLevel)
                        && (row * 48 * zoomLevel + cameraPosition.Y < 1080f)
                        && (row * 48 * zoomLevel + cameraPosition.Y > -48 * zoomLevel))
                    {
                        if(solidGeom[row,col]==0 && zoomLevel>=0.8f)
                            sb.Draw(empty_grid, new Rectangle((int)(col * 48 * zoomLevel) - (int)cameraPosition.X, 1080 - (int)(48 * zoomLevel) - (int)(row * 48 * zoomLevel) - (int)cameraPosition.Y, (int)(48 * zoomLevel), (int)(48 * zoomLevel)), Color.White);

                        if (solidGeom[row, col] == 1 || solidGeom[row, col] == 3)
                            sb.Draw(sketchy_block, new Rectangle((int)(col * 48 * zoomLevel) - (int)cameraPosition.X, 1080 - (int)(48 * zoomLevel) - (int)(row * 48 * zoomLevel) - (int)cameraPosition.Y, (int)(48 * zoomLevel), (int)(48 * zoomLevel)), Color.White);
                            
                    }
                }
            }
            if(editorMode==1)
                DrawBrush();
        }

        private void DrawCrosshair()
        {
            sb.Draw(crosshair, new Rectangle((int)cursorPosition.X - 96, (int)cursorPosition.Y - 96, 192, 192), Color.White);
        }

        private void DrawBrush()
        {
            switch (brushSize)
            {
                case 0:
                    sb.Draw(empty_grid_thick,
                        new Rectangle((int)(selectedGrid.X * 48 * zoomLevel) - (int)cameraPosition.X,
                        1080 - (int)(48 * zoomLevel) - (int)(selectedGrid.Y * 48 * zoomLevel) - (int)cameraPosition.Y,
                        (int)(48 * zoomLevel),
                        (int)(48 * zoomLevel)),
                        Color.White);
                    break;
                case 1:
                    //sb.Draw(brush1, new Rectangle((int)cursorPosition.X - 288, (int)cursorPosition.Y - 288, 576, 576), Color.White);
                    sb.Draw(brush1,
                        new Rectangle((int)((selectedGrid.X-1) * 48 * zoomLevel) - (int)cameraPosition.X,
                        1080 - (int)(48 * zoomLevel) - (int)((selectedGrid.Y + 1) * 48 * zoomLevel) - (int)cameraPosition.Y,
                        (int)(48 * 3 * zoomLevel),
                        (int)(48 * 3* zoomLevel)),
                        Color.White);
                    
                    break;
                case 2:
                    sb.Draw(brush2,
                        new Rectangle((int)((selectedGrid.X - 2) * 48 * zoomLevel) - (int)cameraPosition.X,
                        1080 - (int)(48 * zoomLevel) - (int)((selectedGrid.Y + 2) * 48 * zoomLevel) - (int)cameraPosition.Y,
                        (int)(48 * 5 * zoomLevel),
                        (int)(48 * 5 * zoomLevel)),
                        Color.White);
                    break;
                case 3:
                    sb.Draw(brush3,
                        new Rectangle((int)((selectedGrid.X - 3) * 48 * zoomLevel) - (int)cameraPosition.X,
                        1080 - (int)(48 * zoomLevel) - (int)((selectedGrid.Y + 3) * 48 * zoomLevel) - (int)cameraPosition.Y,
                        (int)(48 * 7 * zoomLevel),
                        (int)(48 * 7 * zoomLevel)),
                        Color.White);
                    break;
            }
        }

        private void EmptyMap()
        {
            entityList.Clear();
            //entityList.Clear();
            //trapList.Clear();

            for (int i = 0; i < solidGeom.GetLength(0); i++)
                for (int j = 0; j < solidGeom.GetLength(1); j++)
                    solidGeom[i, j] = 0;
        }
        private void FilledMap()
        {
            entityList.Clear();
            //entityList.Clear();
            //trapList.Clear();

            for (int i = 0; i < solidGeom.GetLength(0); i++)
                for (int j = 0; j < solidGeom.GetLength(1); j++)
                    solidGeom[i, j] = 1;
        }

        public void NewLevel()
        {
            FilledMap();
        }

        public void StopEditor()
        {
            EmptyMap();
            SetToDefaults();
        }

        private Point GetSelectedGrid()
        {
            Point selectedGrid = new Point();


            selectedGrid.X = (int)Math.Floor((cursorPosition.X + cameraPosition.X) / (48 * zoomLevel));
            selectedGrid.Y = (int)Math.Floor((1080 - cursorPosition.Y - cameraPosition.Y) / (48 * zoomLevel));

            return selectedGrid;
        }

        private void RemoveBlock(Point p)
        {
            if (p.X >= 0 && p.X < solidGeom.GetLength(1) && p.Y >= 0 && p.Y < solidGeom.GetLength(0) && solidGeom[p.Y, p.X] == 1)
                solidGeom[p.Y, p.X] = 0;
        }

        private void UseBrushAdd()
        {
            AddBlock(selectedGrid);
            if (brushSize >= 1)
            {
                AddBlock(new Point(selectedGrid.X - 0, selectedGrid.Y + 1));
                AddBlock(new Point(selectedGrid.X - 0, selectedGrid.Y - 1));
                AddBlock(new Point(selectedGrid.X + 1, selectedGrid.Y - 0));
                AddBlock(new Point(selectedGrid.X - 1, selectedGrid.Y - 0));
            }
            if (brushSize >= 2)
            {
                AddBlock(new Point(selectedGrid.X + 1, selectedGrid.Y + 1));
                AddBlock(new Point(selectedGrid.X - 1, selectedGrid.Y - 1));
                AddBlock(new Point(selectedGrid.X + 1, selectedGrid.Y - 1));
                AddBlock(new Point(selectedGrid.X - 1, selectedGrid.Y + 1));
                AddBlock(new Point(selectedGrid.X - 0, selectedGrid.Y + 2));
                AddBlock(new Point(selectedGrid.X - 0, selectedGrid.Y - 2));
                AddBlock(new Point(selectedGrid.X + 2, selectedGrid.Y - 0));
                AddBlock(new Point(selectedGrid.X - 2, selectedGrid.Y - 0));
            }
            if (brushSize >= 3)
            {
                AddBlock(new Point(selectedGrid.X + 3, selectedGrid.Y + 0));
                AddBlock(new Point(selectedGrid.X - 3, selectedGrid.Y - 0));
                AddBlock(new Point(selectedGrid.X + 0, selectedGrid.Y + 3));
                AddBlock(new Point(selectedGrid.X - 0, selectedGrid.Y - 3));
                AddBlock(new Point(selectedGrid.X + 2, selectedGrid.Y + 1));
                AddBlock(new Point(selectedGrid.X + 2, selectedGrid.Y - 1));
                AddBlock(new Point(selectedGrid.X - 2, selectedGrid.Y + 1));
                AddBlock(new Point(selectedGrid.X - 2, selectedGrid.Y - 1));
                AddBlock(new Point(selectedGrid.X + 1, selectedGrid.Y + 2));
                AddBlock(new Point(selectedGrid.X + 1, selectedGrid.Y - 2));
                AddBlock(new Point(selectedGrid.X - 1, selectedGrid.Y + 2));
                AddBlock(new Point(selectedGrid.X - 1, selectedGrid.Y - 2));
            }
        }

        private void UseBrushRemove()
        {
            RemoveBlock(selectedGrid);
            if (brushSize >= 1)
            {
                RemoveBlock(new Point(selectedGrid.X - 0, selectedGrid.Y + 1));
                RemoveBlock(new Point(selectedGrid.X - 0, selectedGrid.Y - 1));
                RemoveBlock(new Point(selectedGrid.X + 1, selectedGrid.Y - 0));
                RemoveBlock(new Point(selectedGrid.X - 1, selectedGrid.Y - 0));
            }
            if (brushSize >= 2)
            {
                RemoveBlock(new Point(selectedGrid.X + 1, selectedGrid.Y + 1));
                RemoveBlock(new Point(selectedGrid.X - 1, selectedGrid.Y - 1));
                RemoveBlock(new Point(selectedGrid.X + 1, selectedGrid.Y - 1));
                RemoveBlock(new Point(selectedGrid.X - 1, selectedGrid.Y + 1));
                RemoveBlock(new Point(selectedGrid.X - 0, selectedGrid.Y + 2));
                RemoveBlock(new Point(selectedGrid.X - 0, selectedGrid.Y - 2));
                RemoveBlock(new Point(selectedGrid.X + 2, selectedGrid.Y - 0));
                RemoveBlock(new Point(selectedGrid.X - 2, selectedGrid.Y - 0));
            }
            if (brushSize >= 3)
            {
                RemoveBlock(new Point(selectedGrid.X + 3, selectedGrid.Y + 0));
                RemoveBlock(new Point(selectedGrid.X - 3, selectedGrid.Y - 0));
                RemoveBlock(new Point(selectedGrid.X + 0, selectedGrid.Y + 3));
                RemoveBlock(new Point(selectedGrid.X - 0, selectedGrid.Y - 3));
                RemoveBlock(new Point(selectedGrid.X + 2, selectedGrid.Y + 1));
                RemoveBlock(new Point(selectedGrid.X + 2, selectedGrid.Y - 1));
                RemoveBlock(new Point(selectedGrid.X - 2, selectedGrid.Y + 1));
                RemoveBlock(new Point(selectedGrid.X - 2, selectedGrid.Y - 1));
                RemoveBlock(new Point(selectedGrid.X + 1, selectedGrid.Y + 2));
                RemoveBlock(new Point(selectedGrid.X + 1, selectedGrid.Y - 2));
                RemoveBlock(new Point(selectedGrid.X - 1, selectedGrid.Y + 2));
                RemoveBlock(new Point(selectedGrid.X - 1, selectedGrid.Y - 2));
            }
        }

        private void AddBlock(Point p)
        {
            if (p.X >= 0 && p.X < solidGeom.GetLength(1) && p.Y >= 0 && p.Y < solidGeom.GetLength(0) && solidGeom[p.Y, p.X] == 0)
                solidGeom[p.Y, p.X] = 1;
        }

        private void SelectedEntity()
        {
            selectedEntity = false;

            for (int i = 0; i < entityList.Count; i++)
                for (int j = 0; j < entityList[i].size.X; j++)
                    for (int k = 0; k < entityList[i].size.Y; k++)
                        if (selectedGrid.X == entityList[i].gridPos.X + k
                            && selectedGrid.Y == entityList[i].gridPos.Y - j)
                        {
                            tempEntity = entityList[i];
                            selectedEntity = true;
                        }
        }

        private void ReturnSpaceToEmpty()
        {
            for (int i = 0; i < tempEntity.size.Y; i++)
            {
                for (int j = 0; j < tempEntity.size.X; j++)
                    solidGeom[tempEntity.gridPos.Y - j, tempEntity.gridPos.X + i] = 0;
                if (tempEntity.connectionPoint.Equals(Direction.Down))
                    solidGeom[tempEntity.gridPos.Y - tempEntity.size.X, tempEntity.gridPos.X + i] = 1;
            }
        }

        private void RemoveEntity()
        {
            if (selectedEntity)
            {
                for (int i = entityList.Count - 1; i >= 0; i--)
                {
                    if (entityList[i].Equals(tempEntity))
                    {
                        ReturnSpaceToEmpty();
                        entityList.RemoveAt(i);
                    }
                }
                UpdateDangerLevel();
            }
        }

        public void SaveLevel(String name)
        {
            int dir;

            XDocument doc = new XDocument();
            XElement rootElement = new XElement("level");
            XElement attributes = new XElement("attributes");
            XElement grid = new XElement("grid");
            XElement entities = new XElement("entities");

            rootElement.Add(new XAttribute("name", name));
            rootElement.Add(new XAttribute("author", "billy"));

            grid.Add(new XAttribute("cols", solidGeom.GetLength(1)),
                new XAttribute("rows", solidGeom.GetLength(0)));

            for (int i = 0; i < solidGeom.GetLength(0); i++)
            {
                for (int j = 0; j < solidGeom.GetLength(1); j++)
                {
                    if (solidGeom[i, j] == 1 || solidGeom[i, j] == 3)
                        grid.Add(new XElement("coord",
                            new XAttribute("x", j),
                            new XAttribute("y", i),
                            new XAttribute("mat", "stone")));
                }
            }

            foreach (EntityInEditor e in entityList)
            {
                if (e.facing.Equals(Direction.Left))
                    dir = 0;
                else
                    dir = 1;

                entities.Add(new XElement("entity",
                        new XAttribute("name", e.entName),
                        new XAttribute("type", "Character"),
                        new XAttribute("xPos", e.gridPos.X*gridSize),
                        new XAttribute("yPos", e.gridPos.Y * gridSize - (e.size.Y - 1) * gridSize),
                        new XAttribute("direction", dir)));

            }

            rootElement.Add(attributes);
            rootElement.Add(grid);
            rootElement.Add(entities);
            doc.Add(rootElement);

            doc.Save(@"Content/levels/" + name + ".xml");
        }

        public void updateScaleMatrix(Matrix sm)
        {
            scaleMatrix = sm;
        }
    }
}
