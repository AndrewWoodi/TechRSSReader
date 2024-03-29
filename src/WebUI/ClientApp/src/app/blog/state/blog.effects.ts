import { Injectable } from "@angular/core";

import { BlogService } from "../../shared/blog.service";
import { TrainingService } from "../../training/training.service";

/* NgRx */
import { Action } from "@ngrx/store";
import { Actions, createEffect, Effect, ofType } from "@ngrx/effects";
import * as blogActions from "./blog.actions";
import { merge, Observable, of } from "rxjs";
import { mergeMap, map, catchError, concatMap, tap } from "rxjs/operators";
import {
  BlogDto,
  FeedItemUserTagDto,
  UpdateFeedItemCommand,
  UserTagDto,
} from "src/app/techrssreader-api";
import { Router } from "@angular/router";

@Injectable()
export class BlogEffects {
  constructor(
    private blogService: BlogService,
    private trainingService: TrainingService,
    private actions$: Actions,
    private router: Router
  ) {}

  loadAllBlogsLatestSummaries$ = createEffect(() =>
    this.actions$.pipe(
      ofType(blogActions.BlogActionTypes.LoadAllBlogSummaries),
      mergeMap(() =>
        this.blogService.getAllBlogLatestSummaries().pipe(
          map(
            (viewModel) =>
              new blogActions.LoadAllBlogSummariesSuccess(viewModel)
          ),
          catchError((error) =>
            of(new blogActions.LoadAllBlogSummariesFail(error))
          )
        )
      )
    )
  );

  @Effect()
  loadBlogs$: Observable<Action> = this.actions$.pipe(
    ofType(blogActions.BlogActionTypes.LoadBlogs),
    mergeMap(() =>
      this.blogService.getBlogs().pipe(
        map((blogs) => new blogActions.LoadBlogsSuccess(blogs)),
        catchError((error) => of(new blogActions.LoadBlogsFail(error)))
      )
    )
  );

  loadBlogWithItems$ = createEffect(() =>
    this.actions$.pipe(
      ofType(blogActions.BlogActionTypes.LoadBlogWithItems),
      map((action: blogActions.LoadBlogWithItems) => action.payload),
      mergeMap((blogId: number) =>
        this.blogService.getBlogWithFeedItems(blogId).pipe(
          map((blog) => new blogActions.LoadBlogWithItemsSuccess(blog)),
          catchError((error) =>
            of(new blogActions.LoadBlogWithItemsFail(error))
          )
        )
      )
    )
  );

  loadBookmarkedFeedItems$ = createEffect(() =>
    this.actions$.pipe(
      ofType(blogActions.BlogActionTypes.LoadBookmarkedFeedItems),
      mergeMap((action) =>
        this.blogService.getBookmarkedFeedItems().pipe(
          map(
            (viewModel) =>
              new blogActions.LoadBookmarkedFeedItemsSuccess(viewModel)
          ),
          catchError((error) =>
            of(new blogActions.LoadBookmarkedFeedItemsFail(error))
          )
        )
      )
    )
  );

  loadFeedItemDetails$ = createEffect(() =>
    this.actions$.pipe(
      ofType(blogActions.BlogActionTypes.LoadFeedItemDetails),
      map((action: blogActions.LoadFeedItemDetails) => action.payload),
      mergeMap((feedItemId: number) =>
        this.blogService.getFeedItemDetails(feedItemId).pipe(
          map(
            (feedItemDetails) =>
              new blogActions.LoadFeedItemDetailsSuccess(feedItemDetails)
          ),
          catchError((error) =>
            of(new blogActions.LoadFeedItemDetailsFail(error))
          )
        )
      )
    )
  );

  loadTopRatedFeedItems$ = createEffect(() =>
    this.actions$.pipe(
      ofType(blogActions.BlogActionTypes.LoadTopRatedFeedItems),
      mergeMap((action) =>
        this.blogService.getTopRatedFeedItems().pipe(
          map(
            (viewModel) =>
              new blogActions.LoadTopRatedFeedItemsSuccess(viewModel)
          ),
          catchError((error) =>
            of(new blogActions.LoadTopRatedFeedItemsFail(error))
          )
        )
      )
    )
  );

  loadUnreadFeedItems$ = createEffect(() =>
    this.actions$.pipe(
      ofType(blogActions.BlogActionTypes.LoadUnreadFeedItems),
      mergeMap((action) =>
        this.blogService.getUnreadFeedItems().pipe(
          map(
            (viewModel) => new blogActions.LoadUnreadFeedItemsSuccess(viewModel)
          ),
          catchError((error) =>
            of(new blogActions.LoadUnreadFeedItemsFail(error))
          )
        )
      )
    )
  );

  loadUserTags$ = createEffect(() =>
    this.actions$.pipe(
      ofType(blogActions.BlogActionTypes.LoadUserTags),
      mergeMap((action) =>
        this.blogService.getUserTags().pipe(
          map((viewModel) => new blogActions.LoadUserTagsSuccess(viewModel)),
          catchError((error) => of(new blogActions.LoadUserTagsFail(error)))
        )
      )
    )
  );

  loadUserTagFeedItems$ = createEffect(() =>
    this.actions$.pipe(
      ofType(blogActions.BlogActionTypes.LoadUserTagFeedItems),
      map((action: blogActions.LoadUserTagFeedItems) => action.payload),
      mergeMap((userTagId: number) =>
        this.blogService.getTaggedFeedItems(userTagId).pipe(
          map(
            (viewModel) =>
              new blogActions.LoadUserTagFeedItemsSuccess(viewModel)
          ),
          catchError((error) =>
            of(new blogActions.LoadUserTagFeedItemsFail(error))
          )
        )
      )
    )
  );

  loadWeeklyBlogSummaries$ = createEffect(() =>
    this.actions$.pipe(
      ofType(blogActions.BlogActionTypes.LoadWeeklyBlogSummaries),
      map((action: blogActions.LoadWeeklyBlogSummaries) => action.payload),
      mergeMap((blogId: number) =>
        this.blogService.getWeeklyBlogSummaries(blogId).pipe(
          map(
            (viewModel) =>
              new blogActions.LoadWeeklyBlogSummariesSuccess(viewModel)
          ),
          catchError((error) =>
            of(new blogActions.LoadWeeklyBlogSummariesFail(error))
          )
        )
      )
    )
  );

  markItemAsRead$ = createEffect(() =>
    this.actions$.pipe(
      ofType(blogActions.BlogActionTypes.MarkItemAsRead),
      map((action: blogActions.MarkItemAsRead) => action.payload),
      mergeMap((command: UpdateFeedItemCommand) =>
        this.trainingService.updateFeedItem(command).pipe(
          map(
            (feedItem) => new blogActions.MarkItemAsReadSuccess(feedItem),
            catchError((error) => of(new blogActions.MarkItemAsReadFail(error)))
          )
        )
      )
    )
  );

  @Effect()
  retrieveFeedItemsForFeed$: Observable<Action> = this.actions$.pipe(
    ofType(blogActions.BlogActionTypes.RetrieveFeedItemsFromSource),
    map((action: blogActions.RetrieveFeedItemsFromSource) => action.payload),
    concatMap((blogId: number) =>
      this.blogService.retrieveFeedItems(blogId).pipe(
        map(
          (retrievedItemsCount) =>
            new blogActions.RetrieveFeedItemsFromSourceSuccess(
              retrievedItemsCount
            )
        ),
        catchError((error) =>
          of(new blogActions.RetrieveFeedItemsFromSourceFail(error))
        )
      )
    )
  );

  @Effect()
  createBlog$: Observable<Action> = this.actions$.pipe(
    ofType(blogActions.BlogActionTypes.CreateBlog),
    map((action: blogActions.CreateBlog) => action.payload),
    mergeMap((blog: BlogDto) =>
      this.blogService.createBlog(blog).pipe(
        map((newBlog) => new blogActions.CreateBlogSuccess(newBlog)),
        catchError((error) => of(new blogActions.CreateBlogFail(error)))
      )
    )
  );

  createBlogSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(blogActions.BlogActionTypes.CreateBlogSuccess),
        tap((action: blogActions.CreateBlogSuccess) => {
          this.redirectToArticle(action.payload);
        })
      ),
    { dispatch: false }
  );

  createUserTag$ = createEffect(() =>
    this.actions$.pipe(
      ofType(blogActions.BlogActionTypes.CreateUserTag),
      map((action: blogActions.CreateUserTag) => action.payload),
      mergeMap((userTag: UserTagDto) =>
        this.blogService.createUserTag(userTag).pipe(
          map((newUserTag) => new blogActions.CreateUserTagSuccess(newUserTag)),
          catchError((error) => of(new blogActions.CreateUserTagFail(error)))
        )
      )
    )
  );

  createFeedItemUserTag$ = createEffect(() =>
    this.actions$.pipe(
      ofType(blogActions.BlogActionTypes.CreateFeedItemUserTag),
      map((action: blogActions.CreateFeedItemUserTag) => action.payload),
      mergeMap((feedItemUserTag: FeedItemUserTagDto) =>
        this.blogService.createFeedItemUserTag(feedItemUserTag).pipe(
          map(
            (newFeedItemUserTag) =>
              new blogActions.CreateFeedItemUserTagSuccess(newFeedItemUserTag)
          ),
          catchError((error) =>
            of(new blogActions.CreateFeedItemUserTagFail(error))
          )
        )
      )
    )
  );

  @Effect()
  deleteBlog$: Observable<Action> = this.actions$.pipe(
    ofType(blogActions.BlogActionTypes.DeleteBlog),
    map((action: blogActions.DeleteBlog) => action.payload),
    mergeMap((blogId: number) =>
      this.blogService.deleteBlog(blogId).pipe(
        map(() => new blogActions.DeleteBlogSuccess(blogId)),
        catchError((error) => of(new blogActions.DeleteBlogFail(error)))
      )
    )
  );

  deleteFeedItemUserTag$ = createEffect(() =>
    this.actions$.pipe(
      ofType(blogActions.BlogActionTypes.DeleteFeedItemUserTag),
      map((action: blogActions.DeleteFeedItemUserTag) => action.payload),
      concatMap((feedItemUserTag: FeedItemUserTagDto) =>
        this.blogService.deleteFeedItemUserTag(feedItemUserTag).pipe(
          map(
            (deletedFeedItemUserTag) =>
              new blogActions.DeleteFeedItemUserTagSuccess(
                deletedFeedItemUserTag
              )
          ),
          catchError((error) =>
            of(new blogActions.DeleteFeedItemUserTagFail(error))
          )
        )
      )
    )
  );

  deleteBlogSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(blogActions.BlogActionTypes.DeleteBlogSuccess),
        tap((action: blogActions.DeleteBlogSuccess) => {
          this.router.navigate(["/"]);
        })
      ),
    { dispatch: false }
  );

  @Effect()
  updateBlog$: Observable<Action> = this.actions$.pipe(
    ofType(blogActions.BlogActionTypes.UpdateBlog),
    map((action: blogActions.UpdateBlog) => action.payload),
    mergeMap((blog: BlogDto) =>
      this.blogService.updateBlog(blog).pipe(
        map((updatedBlog) => new blogActions.UpdateBlogSuccess(updatedBlog)),
        catchError((error) => of(new blogActions.UpdateBlogFail(error)))
      )
    )
  );

  updateBlogSuccess$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(blogActions.BlogActionTypes.UpdateBlogSuccess),
        tap((action: blogActions.UpdateBlogSuccess) => {
          this.redirectToArticle(action.payload);
        })
      ),
    { dispatch: false }
  );

  toggleFeedItemBookmark$ = createEffect(() =>
    this.actions$.pipe(
      ofType(blogActions.BlogActionTypes.ToggleFeedItemBookmark),
      map((action: blogActions.ToggleFeedItemBookmark) => action.payload),
      mergeMap((command: UpdateFeedItemCommand) =>
        this.trainingService.updateFeedItem(command).pipe(
          map(
            (feedItem) =>
              new blogActions.ToggleFeedItemBookmarkSuccess(feedItem),
            catchError((error) =>
              of(new blogActions.ToggleFeedItemBookmarkFail(error))
            )
          )
        )
      )
    )
  );

  updateUserInterest$ = createEffect(() =>
    this.actions$.pipe(
      ofType(blogActions.BlogActionTypes.UpdateUserInterest),
      map((action: blogActions.UpdateUserInterest) => action.payload),
      mergeMap((command: UpdateFeedItemCommand) =>
        this.trainingService.updateFeedItem(command).pipe(
          map(
            (feedItem) => new blogActions.UpdateUserInterestSuccess(feedItem),
            catchError((error) =>
              of(new blogActions.UpdateUserInterestFail(error))
            )
          )
        )
      )
    )
  );

  redirectToArticle(blog: BlogDto): void {
    this.router.navigate(["/articles", blog.id]);
  }
}
