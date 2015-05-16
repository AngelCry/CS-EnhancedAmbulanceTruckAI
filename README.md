# Enhanced Ambulance AI
Requires: [[ARIS] Skylines Overwatch](https://github.com/arislancrescent/CS-SkylinesOverwatch)

Oversees ambulance services to ensure that garbage trucks are dispatched in an efficient manner.

There are two sets of important concepts to keep in mind for this mod: 

1. Inside vs outside a district
2. Primary vs secondary pickup zone

## Inside a District
If a hospital/clinic is inside a district, then:

1. Its primary pickup zone is the district
2. Its secondary pickup zone is its effective area, which is specified by the game as a radius range with the building as the center. This area is approximately 50% of one tile.

## Outside a District
If a hospital/clinic does not belong to any district, then:

1. Its primary pickup zone include all the areas within its effective area that does not belong to a district
2. Its secondary pickup zone is the rest of its effective area

## Pickup Priority
Ambulances will always prioritize buildings in their primary pickup zone. However, there are several rules within this general rule:

1. Between problematic buildings (those showing the sick sign) and nonproblematic ones (those that have no visual clues), ambulances will prioritize the problematic ones for pickup
2. However, if they come across a nonproblematic one along their way, they will pick it up first; but only if it is not behind them
3. If there is a closer building of the same priority, they will redirect to the closer one; but only if it is along the original bearing 

## Efficiency vs Urgency
The pickup priority above exists to achieve a good balance between making the ambulances as efficient as possible vs keeping your CIMs as happy as possible. When you see a sick sign, that means a building has become a nusance and your CIMs are not happy about it. But if we were to prioritize getting rid of the sick signs as fast as possible, we would have to take a large hit on overall efficiency. On the other hand, if we were to do the opposite, then we would be ensuring maximum efficiency at the risk of losing buildings to abandonment. The existing setup benefits from both approaches by prioritizing problematic buildings for overall bearing, but at the same time allowing pickups of nonproblematic buildings along the way.

## Conflict Resolution
Each hospital/clinic gets its own dispatcher. The dispatcher will try to maximize the efficiency of all its ambulances, i.e., reduce the chance that two ambulances will be sent to the same location for pick up. HOWEVER, just like in real life, the dispatchers of different hospital/clinic will not call each other constantly to make sure they are not all dispatching for the same building. So, if you have multiple clinics right next to each other, it is possible that a building will be fought over by ambulances from different clinics. With that said, it shouldn't be a common occurrence.
