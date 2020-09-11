import { Action } from "@ngrx/store";
import { RssFeedItemDto, UpdateUserInterestedCommand } from "src/app/techrssreader-api";

export enum TrainingActionTypes {
  GetTrainingItem = "[Training GUI] Get Training Item",
  GetTrainingItemFail = "[RSSFeedItem API] Get Training Item Fail",
  GetTrainingItemSuccess = "[RSSFeedItem API] Get Training Item Success",
  UpdateUserInterest = "[Training GUI] Update User Interest",
  UpdateUserInterestFail = "[RSSFeedItem API] Update User Interest Fail",
  UpdateUserInterestSuccess = "[RSSFeedItem API] Update User Interest Success"
}

export class GetTrainingItem implements Action {
  readonly type = TrainingActionTypes.GetTrainingItem;
  constructor(public payload: number) {}
}

export class GetTrainingItemFail implements Action {
  readonly type = TrainingActionTypes.GetTrainingItemFail;
  constructor(public payload: string) {}
}

export class GetTrainingItemSuccess implements Action {
  readonly type = TrainingActionTypes.GetTrainingItemSuccess;
  constructor(public payload: RssFeedItemDto) {}
}

export class UpdateUserInterest implements Action {
  readonly type = TrainingActionTypes.UpdateUserInterest;
  constructor(public payload: UpdateUserInterestedCommand) {}
}

export class UpdateUserInterestFail implements Action {
  readonly type = TrainingActionTypes.UpdateUserInterestFail;
  constructor(public payload: string) {}
}

export class UpdateUserInterestSuccess implements Action {
  readonly type = TrainingActionTypes.UpdateUserInterestSuccess;
  constructor(public payload: RssFeedItemDto) {}
}

export type TrainingActions = GetTrainingItem
| GetTrainingItemFail
| GetTrainingItemSuccess
| UpdateUserInterest
| UpdateUserInterestFail
| UpdateUserInterestSuccess;
