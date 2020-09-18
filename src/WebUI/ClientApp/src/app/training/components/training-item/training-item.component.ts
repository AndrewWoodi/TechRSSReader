import { Component, Input, Output, EventEmitter } from "@angular/core";
import { FormGroup, FormBuilder } from "@angular/forms";

import {
  RssFeedItemDto,
  BlogDto,
  UpdateFeedItemCommand,
} from "src/app/techrssreader-api";

@Component({
  selector: "training-item",
  templateUrl: "./training-item.component.html",
  styleUrls: ["./training-item.component.scss"],
})
export class TrainingItemComponent {
  @Input() currentFeedItem: RssFeedItemDto;
  @Input() selectedBlog: BlogDto;
  @Output() loadItemRequest = new EventEmitter<BlogDto>();
  @Output() userRegisteredInterest = new EventEmitter<
    UpdateFeedItemCommand
  >();

  trainingForm: FormGroup;

  constructor(private formBuilder: FormBuilder) {}

  ngOnInit() {
    this.trainingForm = this.formBuilder.group({
      userRating: 0,
      readAlready: false
    });
  }

  loadFeedItemClicked(): void {
    this.loadItemRequest.emit(this.selectedBlog);
  }

  handleRatingClicked(rating: number)
  {
    this.trainingForm.controls["userRating"].setValue(rating);
    this.trainingForm.markAsDirty();
  }

  saveUserInterest(): void {
      const userRating: number = this.trainingForm.get("userRating").value;
    const command: UpdateFeedItemCommand = UpdateFeedItemCommand.fromJS(
      {
        id: this.currentFeedItem.id,
        readAlready: this.currentFeedItem.readAlready,
        userRating: userRating
      }
    );

    this.userRegisteredInterest.emit(command);
    this.trainingForm.reset();
  }
}
