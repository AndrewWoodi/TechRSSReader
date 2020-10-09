import {
  RssFeedItemsClient,
  RssFeedItemDto,
  UpdateFeedItemCommand
} from "../TechRSSReader-api";
import { Observable } from "rxjs";

export class TrainingService {
  constructor(private rssFeedItemsClient: RssFeedItemsClient) {}

  getTrainingFeedItem(blogId: number): Observable<RssFeedItemDto>{

    return this.rssFeedItemsClient.getNoUserPreference(blogId);
  }

  updateFeedItem(command: UpdateFeedItemCommand): Observable<RssFeedItemDto> {
    return this.rssFeedItemsClient.update(command.id, command);
 }
}
