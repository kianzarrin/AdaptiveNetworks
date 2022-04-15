### modifications after ModifySegmentMask
- start/end at netAI.GetTerrainModifyRange (fraction of total lenght)
- height:
  - += interpolate(m_terrainStartOffset, m_terrainEndOffset)
  - if(m_lowerTerrain) += netAI.GetTerrainLowerOffset()
  - if(netAI.RaiseTerrain()) += m_maxHeight
- surface flags:
  - if(!m_createPavement)
    remove  PavementA
  - if(m_lowerTerrain)
    - remove PavementA
    - except: first or last quad with OnGround flag on node
  - if(PavementA) also Gravel
- edge flags:
  - DA: only on first quad and inverted
  - BC: only on last quad and inverted
- if(netAI.GetTerrainModifyRange doesn't start at 0 and (m_flattenTerrain)||(m_lowerTerrain)&&netAI.RaiseTerrain())
  --> TunnelAI with flatten or lower terrain set
  - duplicate first section with:
    - height:
      - += 0.75*m_terrainStartOffset
    - edge flags:
      - limited to AB and CD (long edges)
      - DA allowed on first quad

---> recommended: set m_flattenTerrain and m_createPavement for maximum control  
---> if you want pavement only near nodes with onGround flag, set m_lowerTerrain and set m_terrainStart/EndOffset to -netAI.GetTerrainLowerOffset()  
---> maybe do that automatically?  
---> maybe pre-invert Edges.BC and DA so that they behave the same as AB and CD?  
### netAI-specific:
- RaiseTerrain()
  - default: false
  - true for: *TunnelAI
- GetTerrainLowerOffset()
  - default: -1.5
  - CableCarPathAI, PowerLineAI, SupportCableAI: max(0, m_minHeight - 3)
  - MonorailTrackAI: max(0, m_minHeight - 1)
  - PedestrianBridgeAI: -0.1
- GetTerrainModifyRange()
  - default: [0,1]
  - *TunnelAI with m_flattenTerrain [0.25,1]
### applyQuad flags
#### edge flags
- AB: left edge
- BC: end edge
- CD: right edge
- DA: start edge
- Expand: ??? (not used afaik) --> won't survive anyway

function:
- for height modification:
  - overextend over that edge (?)
    - by 5.656854m with DetailMapping (inside purchased area)
    - by 22.6274166m without ""
    - by 0m for DigHeight or (SecondaryLevel without PrimaryLevel)
    - PrimaryLevel or SecondaryLevel add 12m
  - some more stuff I don't understand right now, see public static void ApplyQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, TerrainModify.Edges edges, TerrainModify.Heights heights, float[] heightArray)
- for surface modification:
  - appears to do some antialiasing that rendering then uses to create straight edges. This doesn't work once modifications get to close to another or angles become to sharp (most common case is that ped paths "connect" to sidewalks; I used to believe this was related to the connection created in pathfinding; but seems like it's just a glitch that coincides)
#### surface flags
basically Surface Painter mod
- Clip
- PavementA
- PavementB: ??
- Ruined
- Gravel
- Field
- RuinedWeak: ??
#### height flags
- PrimaryLevel --> sets PrimaryMax and PrimaryMin
  - used for regular roads (m_flattenTerrain)
- SecondaryLevel --> sets SecondaryMax and SecondaryMin
- PrimaryMax
  - used for bridges (m_lowerTerrain)
- BlockHeight --> basically WaterMin
  - probably related to water?
- SecondaryMin
  - used when asset says neither flatten nor lower, but AI says Raise
- DigHeight: probably canals? [verify] --> basically WaterMax
- RawHeight: permanent deformation (like landscaping tools) (probably?)
- SecondaryMax

First combined per channel (...Max: lowest contribution wins, if none: 1024f, ..Min: highest contribution wins, if none 0f), then the channels are combined

Priority (most to least powerful):
- finalHeight (visual?): PrimaryMax > PrimaryMin > SecondaryMax > SecondaryMin > weighted average of \*level
- blockHeightTargets (probably water): PrimaryMax > DigHeight (Max) > BlockHeight > finalHeight

(thus, a dam that goes over a ground road won't block water (?))  
(thus, an invisible dam without clipping would/could set BlockHeight and a (lower) SecondaryMax)
